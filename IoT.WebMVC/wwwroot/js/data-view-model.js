/*global ko */

var IoT = this.IoT || {};

(function (namespace) {
    "use strict";
    namespace.dataViewModel = function () {
        var self = this;
        self.logs = ko.observableArray();
        self.addLog = function (data) {
            self.logs.push(data);
        };
        self.temps = ko.observableArray();    
        self.addTemps = function (data) {            
            self.temps.removeAll();
            data.forEach(function (element) {
                self.temps.push(element);
            });          
        };
    };
}(IoT));