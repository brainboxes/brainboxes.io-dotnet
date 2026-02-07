function Chart(elem, attrs) {
    var self = this;
    // public properties
    this.historyMS = parseInt(attrs.historyMs);
    this.scaleMax = parseInt(attrs.scaleMax);
    this.tick = 250;
    this.pathClass = "path";
    this.data;
    this.xScale;
    this.yScale;
    this.xAxisGen;
    this.yAxisGen;
    this.lineFun;
    this.width = 300;
    this.height = 200;
    //private properties
    var d3Elem, svg;

    //init function
    (function () {
        var now = new Date(),
        nowMinusHistory = new Date(now.getTime() - self.historyMS);
        self.data = [
            { time: new Date(nowMinusHistory.getTime()), value: 0 },
            { time: new Date(now.getTime()), value: 0 }
        ];

        d3Elem = d3.select("#" + elem),
            rawSvg = d3Elem.select('svg')
            //.attr('width', width)
            //.attr('height', height)
            .attr('viewBox', '0 0 ' + self.width + ' ' + self.height);
         svg = d3.select(rawSvg[0][0]);

    })();

    function setChartParameters() {
        var dateNow = new Date();

        var thirtySecondsFromNow = new Date(),
            fifteenSecondsBeforeNow = new Date();

        thirtySecondsFromNow.setTime(dateNow.getTime() + 1000 * 15);
        fifteenSecondsBeforeNow.setTime(dateNow.getTime() - 1000 * 15);

        self.xScale = d3.time.scale()
            .domain([fifteenSecondsBeforeNow, thirtySecondsFromNow])
            .range([0, self.width]);

        self.yScale = d3.scale.linear()
            .domain([0, self.scaleMax])
            .range([self.height*0.9, 0]);

        self.xAxisGen = d3.svg.axis()
            .scale(self.xScale)
            .orient("bottom")
            .ticks(4);

        //yAxisGen = d3.svg.axis()
        //    .scale(yScale)
        //    .orient("left")
        //    .ticks(0);

        self.lineFun = d3.svg.line()
            .x(function (d) {
                return self.xScale(d.time);
            })
            .y(function (d) {
                return self.yScale(d.value);
            })
         .interpolate("basic");
    }

    // public functions

    this.setNewValue = function(newValue){
        var now = new Date();
        self.data.push({ time: new Date(now.getTime()), value: newValue  });
        //add two values the same, this makes updating the graph on interval easier
        self.data.push({ time: new Date(now.getTime()), value: newValue  });
        self.redrawLineChart();
    }
               

    this.updateChart = function () {
        var now = new Date(),
            lastNSeconds = new Date(now.getTime() - self.historyMS),
            allowedResolution = new Date(now.getTime() - 250);
        if (self.data[self.data.length - 1].time > allowedResolution) {
            return;
        }
        //function findOvertimeArray(element) {
        //    return element.time > lastNSeconds
        //}
        //if (self.historyMS) {
        //    self.data = self.data.filter(findOvertimeArray);
        //}
        if (self.data[1].time < lastNSeconds) {
            self.data.shift();
        }

     //   self.data[0].time = lastNSeconds;
        //find index of first time in last Nseconds
        //splice array 1 before that
        //if(self.data)
        if (self.data[self.data.length - 1].time < now) {
            self.data[self.data.length - 1].time = now;
        }

        if (self.data.length === 1)
        {
            self.data.push({ time: new Date(now.getTime()), value: self.data[0].value });
        }

        this.redrawLineChart();
    }

    this.drawLineChart = function () {

        setChartParameters();
  
        
        svg.append("svg:g")
            .attr("class", "x axis")
            .attr("transform", "translate(0," + self.height*0.9 + ")")
            .call(self.xAxisGen);
                
        //svg.append("svg:g")
        //   .attr("class", "y axis")
        //    .attr("transform", "translate(20,0)")
        //    .call(self.yAxisGen);
                
        svg.append("svg:path")
            .attr({
                d: self.lineFun(self.data),
                "stroke": "blue",
                "stroke-width": 2,
                "fill": "none",
                "class": self.pathClass
            });
    }

    this.redrawLineChart = function () {

        setChartParameters();

        svg.selectAll("g.x.axis")
             //.duration(self.tick)
             //.ease('linear')
            .call(self.xAxisGen);
                
        svg.selectAll("." + self.pathClass)
            .attr({
                d: self.lineFun(self.data)
            });
    }

    this.drawLineChart();


}