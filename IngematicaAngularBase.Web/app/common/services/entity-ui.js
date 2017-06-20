angular.module('appBase').service('entityUI', function () {

    // Definition and Usage: Genera un nuevo "id" para identificar las entidades nuevas.
    // Return: Devuelve un entero empezando en -1 y restandole 1 por cada registro nuevo pedido / Type: int
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de entidades / Type: Array Objects / Required
    //  obj.listNestedProp: Nombre de lista de entidades anidada  / Type: String / Optional
    //  obj.prop: Nombre de la propiedad de la entidad que representa el "id" / Type: String / Required
    // Precondition: Requiere una lista de objetos con almenos una propiedad, la lista puede ser vacia.

    this.newId = function (obj) {
        var idx = -1;
        if (obj.list && obj.list.length > 0) {
            for (var i = 0; i < obj.list.length; i++) {
                if (obj.arrayNestedProp && obj.list[i][obj.arrayNestedProp] && obj.list[i][obj.arrayNestedProp].length > 0) {
                    for (var j = 0; j < obj.list[i][obj.arrayNestedProp].length; j++) {
                        if (obj.list[i][obj.arrayNestedProp][j][obj.prop] < idx)
                            idx = obj.list[i][obj.arrayNestedProp][j][obj.prop];
                    }
                }
                else {
                    if (obj.list[i][obj.prop] < idx)
                        idx = obj.list[i][obj.prop];
                }
            }
            if (idx > 0) idx = -1; else idx--;
        }
        return idx;
    };



    // Definition and Usage: Busca una entidad en una lista dada.
    // Return: Devuelve la entidad buscada, una copia de esta o null en caso de no ser encontrada Type: object
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de entidades / Type: Array Objects / Required
    //  obj.prop: Nombre de la propiedad de la entidad a buscar / Type: String / Required
    //  obj.propValue: Valor a buscar en la propiedad dada / Type: Any Type / Required
    //  obj.returnCopy: Retorna una copia del objeto / Type: Boolean / Optional  
    //  obj.caseSensitive: Especifica si al comparar se tiene en cuenta mayusculas y minusculas / Type: Boolean / Optional
    // Precondition: Requiere una lista de objetos con al menos una propiedad, la lista puede ser vacia.

    this.findEntity = function (obj) {
        for (var i = 0; i < obj.list.length; i++) {
            var a = obj.list[i][obj.prop];
            var b = obj.propValue;
            if (typeof(a) == 'string' && typeof(b) == 'string' && (!obj.caseSensitive || obj.caseSensitive != true)) {
                a = a.toLowerCase();
                b = b.toLowerCase();
            }
            if (a == b) {
                if (obj.returnCopy)
                    return angular.copy(obj.list[i]);
                else
                    return obj.list[i];
            }
        }
        return null;
    };

    // Definition and Usage: Busca una entidad en una lista dada por N propiedades.
    // Return: Devuelve la entidad buscada, una copia de esta o null en caso de no ser encontrada Type: object
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de entidades / Type: Array Objects / Required
    //  obj.props: Array con los nombres de la propiedades de la entidad a buscar / Type: String / Required
    //  obj.propsValue: Array con los valores a buscar en las propiedad dada / Type: Any Type / Required
    //  obj.returnCopy: Retorna una copia del objeto / Type: Boolean / Optional
    // Precondition: Requiere una lista de objetos con al menos una propiedad y las listas de propiedades,
    // y valores deben tener el mismo tamaño, la lista puede ser vacia.

    this.findEntityEx = function (obj) {
        for (var i = 0; i < obj.list.length; i++) {
            var result = true;
            for (var z = 0; z < obj.props.length; z++) {
                if (obj.list[i][obj.props[z]] != obj.propsValue[z]) {
                    result = false;
                    break;
                }
            }
            if (result)
                if (obj.returnCopy)
                    return angular.copy(obj.list[i]);
                else
                    return obj.list[i];
        }
        return null;
    };

    // Definition and Usage: Busca una entidad en una lista dada y devuelve el valor de un atributo de la misma.
    // Return: Devuelve la propiedad especificada de la entidad encontrada o el defaultValue en caso negativo. / Type: Any Type 
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de entidades / Type: Array Objects / Required
    //  obj.prop: Nombre de la propiedad de la entidad a buscar / Type: String / Required 
    //  obj.propValue: Valor a buscar en la propiedad dada 'prop' / Type: Any Type / Required
    //  obj.propResult: Nombre de la propiedad de retorno / Type: Any Type / Required
    //  obj.defaultValue: Valor por defecto a retornar si no se encuentra la entidad buscada. / Optional
    // Precondition: Requiere una lista de objetos con al menos dos propiedades,
    // el valor null sera aceptado pero en tal caso se devolver el default value, la lista puede ser vacia.

    this.lookupEntity = function (obj) {
        var result = obj.defaultValue ? obj.defaultValue : '';
        if (obj.propValue && obj.propValue != null) {
            for (var i = 0; i < obj.list.length; i++) {
                if (obj.list[i][obj.prop] == obj.propValue) {
                    result = obj.list[i][obj.propResult];
                    break;
                }
            }
        }
        return result;
    };


    // Definition and Usage:  Reemplaza una entidad dada, bucando en una lista de entidades por un valor.
    // Return: Retorna true si fue hecho el reemplazo o false en caso contrario Type: boolean
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de entidades / Type: Array Objects / Required
    //  obj.prop: Nombre de la propiedad de la entidad a buscar / Type: String / Required
    //  obj.replaceEntity: Entidad a remplazar/ Type: Object / Required
    // Precondition: Requiere una lista de objetos con al menos una propiedad, la lista puede ser vacia.

    this.replaceEntity = function (obj) {
        for (var i = 0; i < obj.list.length; i++) {
            if (obj.list[i][obj.prop] == obj.replaceEntity[obj.prop]) {
                obj.list[i] = obj.replaceEntity;
                return true;
            }
        }
        return false;
    };

    // Definition and Usage:  Elimina una entidad dada, buscando en una lista de entidades por un valor.
    // Return: Retorna true si fue eliminada la entidad o false en caso contrario Type: boolean
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de entidades / Type: Array Objects / Required
    //  obj.prop: Nombre de la propiedad de la entidad a buscar / Type: String / Required
    //  obj.propValue: Valor a buscar en la propiedad dada 'prop' / Type: Any Type / Required
    // Precondition: Requiere una lista de objetos con al menos una propiedad, la lista puede ser vacia.

    this.deleteEntity = function (obj) {
        for (var i = 0; i < obj.list.length; i++) {
            if (obj.list[i][obj.prop] == obj.propValue) {
                obj.list.splice(i, 1);
                return true;
            }
        }
        return false;
    };

    // Definition and Usage: Prepara una lista para ser usada en un Input Select pudiendole agregar un item
    // que no este mas activo y un item con valor null multiproposito.
    // Return: Retorna la lista preparada para ser bindeada a un select Type: Array object
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de items para ser bindeadas a un select / Type: Array Objects / Required
    //  obj.required: La lista se usa en un campo requerido / Type: String / Required
    //  obj.itemId: Identificador del item que podria esta inactivo / Type: Object / Optional
    //  obj.itemDesc: Descripcion del item que podria esta inactivo / Type: String / Optional
    //  obj.nullItem: Se insertara al comienzo de la lista un item  / Type: string / Optional
    //                  'addUpdate' ->  "Seleccione un item"
    //                  'filter' ->  "Todos"                
    // Precondition: Requiere una liste de items con la siguiente estructura
    // {[{id: 1, desc: 'desc1'}, {id: 2, desc: 'desc2'}]}

    this.prepareSelectList = function (obj) {
        if (obj.itemId && obj.itemDesc) {
            var insertedItem = false;
            var itemDesc = obj.itemDesc.toLowerCase();
            var itemDesc2;
            var item = { id: obj.itemId, desc: obj.itemDesc };
            for (var i = 0; i < obj.list.length; i++) {
                itemDesc2 = obj.list[i].desc.toLowerCase();
                if (itemDesc == itemDesc2) {
                    insertedItem = true;
                    break;
                }
                if (itemDesc < itemDesc2) {
                    obj.list.splice(i, 0, item);
                    insertedItem = true;
                    break;
                }
            }
            if (!insertedItem)
                obj.list.push(item);
        }
        if (obj.nullItem) {
            if (obj.list.length > 1 || (!obj.required && obj.list.length))
                if (obj.nullItem == 'addUpdate')
                    obj.list.splice(0, 0, { id: null, desc: "Seleccione un item" });
                else if (obj.nullItem == 'filter')
                    obj.list.splice(0, 0, { id: null, desc: "Todos" });
        }
        return obj.list;
    };

    // Definition and Usage: Dada un lista para un Input Select y el valor a seleccionar, retorna el
    // mismo valor o el valor default segun la cantidad de elementos y si el campo de destino es requerido.
    // que no podria no estar mas activo y un item con valor null multiproposito
    // Return: Retorna el valor dado o el default segun corresponda
    // Parameters:
    //  obj: Objeto con los parametros / Type: Object
    //  obj.list: Array con la lista de items para ser bindeadas a un select / Type: Array Objects / Required
    //  obj.required: La lista se usa en un campo requerido / Type: String / Required
    //  obj.itemId: Valor a seleccionar de la lista / Type: Object / Required              
    // Precondition: Requiere una liste de items con la siguiente estructura
    // {[{id: 1, desc: 'desc1'}, {id: 2, desc: 'desc2'}]}


    this.getSelectItemOrDefault = function (obj) {
        if (obj.required && obj.list.length == 1)
            return obj.list[0].id;
        else
            return obj.itemId == undefined ? null : obj.itemId;
    };



    this.clearObject = function (obj) {
        for (var property in obj) {
            obj[property] = '';
        }
    }

    this.isEmpty = function (obj) {


        if (obj == null) return true;
        if (obj.length > 0) return false;
        if (obj.length === 0) return true;

        if (Object.prototype.toString.call(obj) === '[object Date]') {
            return false;
        }
        else if (typeof obj == 'object') {
            if (Object.getOwnPropertyNames(obj).length > 0) return false;
            return true;
        }
        else {
            if (obj == '') return true;
            return false;
        }
        

       
    }

    this.preparePost = function (object) {                  // completa todos los campos del objeto que sean string nulos a null, de esta manera permite que se pueda enviar a la bdd
        for (var x in object) {
            if (typeof object[x] == 'object') {
                this.preparePost(object[x]);
            }
            if (typeof object[x] == 'string' && object[x] == '') {
                object[x] = null;
            }
        }
    }

    // Definition and Usage: Crea una lista por default para la búsqueda de entidades que 
    // solo pueden tener estado Activo o Inactivo.
    // Return: Retorna la lista creada.
    // Parameters: No corresponde.   
    // Precondition:  No corresponde.

    this.createEstadoSelectList = function () {
        var stateCombo = [];
        stateCombo.push({ "id": 0, "desc": "Todos", "value": null });
        stateCombo.push({ "id": 1, "desc": "Activo", "value": true });
        stateCombo.push({ "id": 2, "desc": "Inactivo", "value": false });
        return stateCombo;
    };


    // Definition and Usage: Crea una lista por default para la búsqueda de entidades que 
    // solo puedan ser Documento Manual o Automatico
    // Return: Retorna la lista creada.
    // Parameters: No corresponde.   
    // Precondition:  No corresponde.

    this.createComprobanteSelectList = function () {
        var stateCombo = [];
        stateCombo.push({ "id": 0, "desc": "Todos", "value": null });
        stateCombo.push({ "id": 1, "desc": "Comprobantes manuales", "value": true });
        stateCombo.push({ "id": 2, "desc": "Comprobantes automaticos", "value": false });
        return stateCombo;
    };

    this.paseDate = function (date) {
        var timestamp, struct, minutesOffset = 0;
        var numericKeys = [1, 4, 5, 6, 7, 10, 11];

        // ES5 §15.9.4.2 states that the string should attempt to be parsed as a Date Time String Format string
        // before falling back to any implementation-specific date parsing, so that’s what we do, even if native
        // implementations could be faster
        //              1 YYYY                2 MM       3 DD           4 HH    5 mm       6 ss        7 msec        8 Z 9 ±    10 tzHH    11 tzmm
        if ((struct = /^(\d{4}|[+\-]\d{6})(?:-(\d{2})(?:-(\d{2}))?)?(?:T(\d{2}):(\d{2})(?::(\d{2})(?:\.(\d{3}))?)?(?:(Z)|([+\-])(\d{2})(?::(\d{2}))?)?)?$/.exec(date))) {
            // avoid NaN timestamps caused by “undefined” values being passed to Date.UTC
            for (var i = 0, k; (k = numericKeys[i]) ; ++i) {
                struct[k] = +struct[k] || 0;
            }

            // allow undefined days and months
            struct[2] = (+struct[2] || 1) - 1;
            struct[3] = +struct[3] || 1;

            /* if (struct[8] !== 'Z' && struct[9] !== undefined) {
                 minutesOffset = struct[10] * 60 + struct[11];
 
                 if (struct[9] === '+') {
                     minutesOffset = 0 - minutesOffset;
                 }
             }*/

            return new Date(struct[1], struct[2], struct[3], struct[4], struct[5] + minutesOffset, struct[6], struct[7]);
        }
        else {
            if (date instanceof Date)
                return date;
            else
                return NaN;
        }

    };

    this.getSizeAssociativeArray = function (obj) {
        var size = 0, key;
        for (key in obj) {
            if (obj.hasOwnProperty(key)) size++;
        }
        return size;
    };



});
