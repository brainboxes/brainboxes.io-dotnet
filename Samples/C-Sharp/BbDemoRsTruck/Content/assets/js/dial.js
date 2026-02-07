'use strict';

function Dial (elementId, initialValue, innerRadius, outerRadius, startAngle, endAngle, clickable, outputChange, maxValue, minValue) {

    this.convertToRadians = function (value, d, e, s) {
        var r;
        d = d || 100;
        e = e || 360;
        s = s || 0;
        r = e - s;
        return (s + ((r / d) * value)) * (Math.PI / 180);
    }

    this.element = d3.select(elementId);
    this.value = isNaN(initialValue) ? 0 : initialValue;

    this.radians = this.convertToRadians(initialValue);
    this.innerRadius = innerRadius;
    this.outerRadius = outerRadius;
    this.startAngle = startAngle,
    this.endAngle = endAngle,
    this.offset = this.outerRadius + 20;
    this.inDrag = false;
    this.clickable = clickable;
    this.gmd = {};
    this.outputChange = outputChange;
    this.maxValue = parseInt(maxValue);
    this.minValue = parseInt(minValue);





    /**
     * @constructor
     *
     * @param {Element} element
     * @param {Number} initialValue 0-100
     * @param {Number} innerRadius
     * @param {Number} outerRadius
     * @param {Number} startAngle
     * @param {Number} endAngle
     */
    //var Knob = function (element, initialValue, innerRadius, outerRadius, startAngle, endAngle, clickable) {
    //    this.element = element;
    //    this.value = initialValue;
    //    this.radians = this.convertToRadians(initialValue);
    //    this.innerRadius = innerRadius;
    //    this.outerRadius = outerRadius;
    //    this.startAngle = startAngle,
    //    this.endAngle = endAngle,
    //    this.offset = this.outerRadius + 20;
    //    this.inDrag = false;
    //    this.clickable = clickable;
    //};

    /**
     * Create the arcs required for this interactive component.
     * 
     * @return {void}
     */
    this.createArcs = function () {
        this.changeArc = createArc(
          this.innerRadius, this.outerRadius, this.convertToRadians(this.startAngle, 360), this.convertToRadians(this.startAngle, 360)
        );
        this.valueArc = createArc(
          this.innerRadius, this.outerRadius, this.convertToRadians(this.startAngle, 360), this.convertToRadians(this.startAngle, 360)
        );
        this.interactArc = createArc(
          this.innerRadius, this.outerRadius, this.convertToRadians(this.startAngle, 360), this.convertToRadians(this.endAngle, 360)
        );

        function createArc(innerRadius, outerRadius, startAngle, endAngle) {
            var arc = d3.svg.arc()
            .innerRadius(innerRadius)
            .outerRadius(outerRadius)
            .startAngle(startAngle);

            if (typeof endAngle !== "undefined") {
                arc.endAngle(endAngle);
            }

            return arc;
        };
    }

    /**
     * Convert a value in [0,100] to radians
     * 
     * @param  {Number} value
     * @param  {Number} d
     * @param  {Number} e
     * @param  {Number} s
     * 
     * @return {Number}
     */


    /**
     * Convert from radians to a value in range [0,100]
     * 
     * @param  {Number} radians
     * @param  {Number} d
     * @param  {Number} e
     * @param  {Number} s
     * 
     * @return {Number}
     */
    this.convertFromRadians = function (radians, d, e, s) {
        var r;
        d = d || 100;
        e = e || 360;
        s = s || 0;
        r = e - s;
        return Math.round(((180 / Math.PI) * Math.abs(radians)) * (d / r));
    }

    /**
     * Append an SVG to the element and draw the dial component
     * 
     * @param  {Function} updateFn
     * @param {Boolean} isAnimated
     * 
     * @return {void}
     */
    //this.draw = function (updateFn, isAnimated) {
    this.draw = function (isAnimated) {
        var self = this;
        self.createArcs();

        var svg = self.element.append('svg').attr("viewBox", "0 0 " + self.offset * 2 +" " + 177);
        svg.attr("height", "350");
        //svg = self.element.append('svg').attr("(click)", "onDrop($event, value)");

        var changeElem = drawArc(self.changeArc, 'changeArc')
        var valueElem = drawArc(self.valueArc, 'valueArc')

        var dragBehavior = d3.behavior.drag()
        .on('drag', clickInteraction);
       // .on('drag', dragInteraction)
       // .on('dragend', clickInteraction);

        drawArc(self.interactArc, 'interactArc', clickInteraction, dragBehavior);

        if (isAnimated) {
            animate(self.convertToRadians(self.startAngle, 360), self.convertToRadians(self.value, 100, self.endAngle, self.startAngle));
        } else {
            self.changeArc.endAngle(this.convertToRadians(this.value, 100, this.endAngle, this.startAngle));
            changeElem.attr('d', self.changeArc);
            self.valueArc.endAngle(this.convertToRadians(this.value, 100, this.endAngle, this.startAngle));
            valueElem.attr('d', self.valueArc);
        }

        svg.append('text')
        .attr('class', 'text')
        .attr('id', 'text')
        .text((self.value * ((self.maxValue - self.minValue) / 100)) + self.minValue)
        .attr('transform', 'translate(' + (self.offset - 14) + ',  24)'); //+ ',  ' + (self.offset + 2) + ')');

        function drawArc(arc, label, click, drag) {
            var elem = svg.append('path')
            .attr('class', label)
            .attr('id', label)
            .attr('d', arc)
            .attr('transform', 'translate(' + (self.offset) + ',  0)'); // + ', ' + (self.offset) + ')');

            if (click) {
                elem.on('click', click);
            }

            if (drag) {
                elem.call(drag);
            }

            return elem;
        }

        function animate(start, end) {

            valueElem
            .transition()
            .ease('bounce')
            .duration(1000)
            .tween('', function () {
                var i = d3.interpolate(start, end);
                return function (t) {
                    var val = i(t);
                    valueElem.attr('d', self.valueArc.endAngle(val));
                    changeElem.attr('d', self.changeArc.endAngle(val));
                };
            });
        }

        function dragInteraction() {
            if (self.clickable) {
                self.inDrag = true;
                var x = d3.event.x - self.offset;
                var y = d3.event.y;// - self.offset;
                interaction(x, y, false);
            }
        }

        function clickInteraction() {
            if (self.clickable) {
                self.inDrag = false;
                var coords = d3.mouse(this.parentNode);
                var x = coords[0] - self.offset;
                var y = coords[1];// - self.offset;
                interaction(x, y, true);
            }
        }

        function interaction(x, y, isFinal) {
        	var arc = Math.atan(y / x) / (Math.PI / 180), radians, delta, originalValue;
            if ((x >= 0 && y <= 0) || (x >= 0 && y >= 0)) {
                delta = 90;
            } else {
                delta = 270;
            }
            radians = ((delta - self.startAngle) + arc) * (Math.PI / 180);
            originalValue = self.value;
            self.value = self.convertFromRadians(radians, 100, self.endAngle, self.startAngle);

            if (self.value != originalValue && self.value >= 0 && self.value <= 100) {
                //updateFn(self.value);
                self.valueArc.endAngle(self.convertToRadians(self.value, 100, self.endAngle, self.startAngle));
                self.element.select('#valueArc').attr('d', self.valueArc);
                if (isFinal) {
                    self.changeArc.endAngle(self.convertToRadians(self.value, 100, self.endAngle, self.startAngle));
                    self.element.select('#changeArc').attr('d', self.changeArc);
                }
                
                self.element.attr('value', self.value);

                var formattedValue = (self.value * ((self.maxValue - self.minValue) / 100)) + self.minValue;
                var stringFormattedValue = (Math.round(formattedValue * 1000) / 1000).toFixed(2);
                self.element.select('#text').text(stringFormattedValue);
                //if (isFinal) {
                //	console.log(elementId + " emit new value: "+formattedValue);
                	self.outputChange.emit({ value: formattedValue });
                //}
            }
        }
    }

    /**
     * Set the value of the gauge to something new.
     * 
     * @param {Number} newValue
     */
    this.setValue = function (newValue) {
    	//console.log(elementId+" dail.setValue");
    	if ((!this.inDrag) && newValue >= 0 && newValue <= 100 && newValue != this.value) {
    		//console.log(elementId + " dail.setValue valid " + newValue);
            var radians = this.convertToRadians(newValue, 100, this.endAngle, this.startAngle);
            this.value = newValue;
            this.changeArc.endAngle(radians);
            this.element.select('#changeArc').attr('d', this.changeArc);
            this.valueArc.endAngle(radians);
            this.element.select('#valueArc').attr('d', this.valueArc);
            var formattedValue = (this.value * ((this.maxValue - this.minValue) / 100)) + this.minValue;
            formattedValue = (Math.round(formattedValue * 1000) / 1000).toFixed(2);
            this.element.select('#text').text(formattedValue);
        }
    }

    //this.gmd.Knob = Knob;

    //this.gmd.dialDirective = function () {
    //    return {
    //        restrict: 'E',
    //        scope: {
    //            value: '='
    //        },
    //        link: function (scope, element, attrs) {
    //            var innerRadius = parseInt(attrs.innerRadius, 10) || 60,
    //                outerRadius = parseInt(attrs.outerRadius, 10) || 100,
    //                startAngle = parseInt(attrs.startAngle, 10) || 0,
    //                endAngle = parseInt(attrs.endAngle, 10) || 360,
    //                clickable = (attrs.clickable === 'false') ? false : true,
    //                knob = new gmd.Knob(element[0], scope.value, innerRadius, outerRadius, startAngle, endAngle, clickable);

    //            function update(value) {
    //                scope.$apply(function () {
    //                    scope.value = value;
    //                });
    //            }

    //            scope.$watch('value', function (newValue, oldValue) {
    //                if ((newValue !== null || typeof newValue !== 'undefined') && typeof oldValue !== 'undefined' && newValue !== oldValue) {
    //                    knob.setValue(newValue);
    //                }
    //            });

    //            knob.draw(update, attrs.animate === "true");
    //        }
    //    };
    //}
      
};