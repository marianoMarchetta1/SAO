

//var bind = Function.bind;
//var unbind = bind.bind(bind);

//function instantiate(constructor, args) {
//    return new (unbind(constructor, null).apply(null, args));
//}

//Date = function (Date) {
//    return function () {
//        var date = instantiate(Date, arguments);

//        var regexIsoString = /^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}$/;

//        if (arguments.length == 1 &&
//            typeof arguments[0] == 'string' ) {
//            date.setYear(parseInt(arguments[0].substr(0, 4)));
//            date.setDate(1); //Seteo primero un dia por default, especifiacamente el 1ro para evitar problemas con meses como 
//            //febrero que tiene menos dias entre 30 y 31, de esta manera el SetMonth no te setea el mes con errores
//            date.setMonth(parseInt(arguments[0].substr(5, 2)) - 1);
//            date.setDate(parseInt(arguments[0].substr(8, 2)));
//            date.setHours(parseInt(arguments[0].substr(11, 2)));
//            date.setMinutes(parseInt(arguments[0].substr(14, 2)));
//            date.setSeconds(parseInt(arguments[0].substr(17, 2)));
//        };


//        return date;
//    }
//}(Date);

function pad(number) {
    if (number < 10) {
        return '0' + number;
    }
    return number;
}

Date.prototype.toISOString = function () {
    return this.getFullYear() +
      '-' + pad(this.getMonth() + 1) +
      '-' + pad(this.getDate()) +
      'T' + pad(this.getHours()) +
      ':' + pad(this.getMinutes()) +
      ':' + pad(this.getSeconds()) +
      '.' + (this.getMilliseconds() / 1000).toFixed(3).slice(2, 5) +
      'Z';
};