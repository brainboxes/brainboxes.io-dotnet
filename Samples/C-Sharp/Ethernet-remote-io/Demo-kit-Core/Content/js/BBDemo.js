angular.module('BBDemo', [])

.constant('imagePath', "/img/")
.constant('sendCommandUrl', "/SendCommand")
.constant('wsUrl', (function () {
        return "ws://" + window.location.hostname+":8989/sync";
    })()
)

.run(function ($rootScope, socket) {
    $rootScope.EDDevice = {
        //ED-527
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
            if (!angular.equals($rootScope.EDDevice, newState))
            {
                $rootScope.ServerMadeLastChange = true;
                $rootScope.EDDevice = newState;
            }

        });
    };
    ws.onclose = function () {
        // websocket is closed.
        $log.log("Socket is closed...");
        if (stopWatchingChanges)
        {
            stopWatchingChanges();
            stopWatchingChanges == null;
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


    $scope.incrementNumber = function () {
        if ($scope.EDDevice.isSliderRight) return; //error state
        $scope.EDDevice.currentNumber = ($scope.EDDevice.currentNumber+ 1 )%10;
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

});
