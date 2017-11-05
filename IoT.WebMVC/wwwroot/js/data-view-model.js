/*global ko */

var IoT = this.IoT || {};

(function (namespace) {
    "use strict";
    namespace.dataViewModel = function () {
        var self = this;

        self.data = ko.observableArray();    
        self.addData = function (data) {            
            self.data.removeAll();
            data.forEach(function (element) {
                self.data.push(element);
            });          
        };
    };
}(IoT));