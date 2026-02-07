angular.module('BBDemo', [])

.constant('imagePath', "img/")
.constant('sendCommandUrl', "/SendCommand")
.constant('wsUrl', (function () {
    return "ws://" + window.location.hostname + ":8989/sync";
})()
)

.run(function ($rootScope, socket) {
	$rootScope.EDDevice = {
		rollingTactTime: "00:00:00.00",
		rollingAvailability: 0,
		//ED-527
		absoluteCount: 0, 
		currentNumber: 0, //DOUT0 - DOUT7
		isFanOn: false, //DOUT8
		isBeeping: false, //DOUT9

		//ED-204
		isSliderRight: false //DIN3
	};
	$rootScope.ServerMadeLastChange = false;
})

.factory('socket', function (wsUrl, $log, $rootScope) {

	var stopWatchingChanges,
	// Let us open a web socket
	ws = new WebSocket(wsUrl);
	ws.onopen = function () {
		// Web Socket is connected, send data using send()
		$log.log("Socket Open");
		stopWatchingChanges = $rootScope.$watchCollection("EDDevice", ws.sync);
	};
	//when a message arrives at the browser from the server
	ws.onmessage = function (evt) {
		$rootScope.$apply(function () {
			var received_msg = evt.data,
				newState = angular.fromJson(evt.data);
			$log.log("State is received...");
			$log.log(received_msg);

			//if the incoming state is different from the current
			//state then the server made the last change
			//and we dont want to trigger another sync
			if (!angular.equals($rootScope.EDDevice, newState)) {
				$rootScope.ServerMadeLastChange = true;
				$rootScope.EDDevice = newState;
			}

		});
	};
	ws.onclose = function () {
		// websocket is closed.
		$log.log("Socket is closed...");
		if (stopWatchingChanges) {
			stopWatchingChanges();
			stopWatchingChanges = null;
		}
	};

	ws.sync = function (oldValue, newValue) {
		if ($rootScope.ServerMadeLastChange) { //debounce state
			$rootScope.ServerMadeLastChange = false;
			return;
		}
		var state = angular.toJson($rootScope.EDDevice);
		$log.info("sending device state ");
		$log.info(state);
		ws.send(state);
	};

	return ws;
})

.controller('demoCtrl', function ($scope, imagePath, $timeout) {

	$scope.numberOfBeeps = 0;

	$scope.$watch("EDDevice.isBeeping", function (oldValue, newValue) {
		if (newValue === true) {
			$scope.numberOfBeeps++;
		}
	});

	$scope.incrementNumber = function () {
		if ($scope.EDDevice.isSliderRight) return; //error state
		$scope.EDDevice.currentNumber = ($scope.EDDevice.currentNumber + 1) % 16;
	};

	$scope.toggleFan = function () {
		$scope.EDDevice.isFanOn = !$scope.EDDevice.isFanOn;
	};

	$scope.makeBeep = function () {

		if ($scope.EDDevice.isBeeping) return;

		$scope.EDDevice.isBeeping = true;

		$timeout(function () {
			$scope.EDDevice.isBeeping = false;
		}, 2500);
	};

	$scope.imagePath = imagePath;

})



.directive('liveChart', function ($parse, $window, $interval) {
	return {
		restrict: 'EA',
		template: "<svg></svg>",
		link: function (scope, elem, attrs) {
			var historyMS = $parse(attrs.historyMs)(),
				chartWatch = $parse(attrs.chartWatch),
				scaleMax = $parse(attrs.scaleMax)(),
				tick = 250,
				padding = 20,
				pathClass = "path",
				now = new Date(),
				nowMinusHistory = new Date(now.getTime() - historyMS),
				//initialise data
				data = [
					{ time: new Date(nowMinusHistory.getTime()), value: 0 },
					{ time: new Date(now.getTime()), value: 0 },
				],
				xScale, yScale, xAxisGen, yAxisGen, lineFun;

			var d3 = $window.d3,
				width = $(elem).width(),
				height = $(elem).height(),
				rawSvg = elem.find('svg')
				.attr('width', width)
				.attr('height', height)
				.attr('viewport', '0 0 ' + width + ' ' + height),
				svg = d3.select(rawSvg[0]);


			scope.$watch(attrs.chartWatch, function (newValue, oldValue) {
				var now = new Date();
				data.push({ time: new Date(now.getTime()), value: newValue });
				//add two values the same, this makes updating the graph on interval easier
				data.push({ time: new Date(now.getTime()), value: newValue });
				redrawLineChart();
			});

			//move the graph along
			$interval(function () {

				var now = new Date(),
				lastFiveMins = new Date(now.getTime() - historyMS);

				if (data[1].time < lastFiveMins) {
					data.shift();
				}

				data[0].time = lastFiveMins;


				if (data[data.length - 1].time < now) {
					data[data.length - 1].time = now;
				}

				redrawLineChart();
			}, tick, 0);

			function setChartParameters() {
				var thirtySecondsFromNow = new Date();
				thirtySecondsFromNow.setTime(thirtySecondsFromNow.getTime() + 1000 * 30);
				xScale = d3.time.scale()
					.domain([data[0].time, thirtySecondsFromNow])
					.range([padding + 5, rawSvg.attr("width") - padding]);

				yScale = d3.scale.linear()
					.domain([0, scaleMax])
					.range([rawSvg.attr("height") - padding, 0]);

				xAxisGen = d3.svg.axis()
					.scale(xScale)
					.orient("bottom")
					.ticks(4);

				//  yAxisGen = d3.svg.axis()
				//      .scale(yScale)
				//      .orient("left")
				//      .ticks(0);

				lineFun = d3.svg.line()
					.x(function (d) {
						return xScale(d.time);
					})
					.y(function (d) {
						return yScale(d.value);
					})
					.interpolate("step-after");
			}

			function drawLineChart() {

				setChartParameters();

				svg.append("svg:g")
					.attr("class", "x axis")
					.attr("transform", "translate(0," + rawSvg.attr("height") / 2 + ")")
					.call(xAxisGen);

				//svg.append("svg:g")
				//   .attr("class", "y axis")
				//    .attr("transform", "translate(20,0)")
				//    .call(yAxisGen);

				svg.append("svg:path")
					.attr({
						d: lineFun(data),
						"stroke": "blue",
						"stroke-width": 2,
						"fill": "none",
						"class": pathClass
					});
			}

			function redrawLineChart() {

				setChartParameters();

				svg.selectAll("g.x.axis")
					// .duration(tick)
					// .ease('linear')
					.call(xAxisGen);

				svg.selectAll("." + pathClass)
					.attr({
						d: lineFun(data)
					});
			}

			drawLineChart();
		}
	};
});