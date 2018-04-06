

function SortByKeyDesc(list, propertyKey) {
    list.sort(function (left, right) {
        return left[propertyKey] == right[propertyKey] ? 0 :
            left[propertyKey] > right[propertyKey] ? -1 : 1
    });
}

function GetByProperty(list, property, targetValue) {
    if (ko.isObservable(list))
        list = list();
    for (var i = 0; i < list.length; i++) {
        var item = list[i];
        var value = item[property];

        if (value === targetValue) {
            return item;
        }
    }
    console.log("Item not found within list with key " + targetValue)
    return undefined;
}

function Project(list, property) {
    var set = new Set();

    if (ko.isObservable(list)) {
        list().forEach(function (i) {
            if (ko.isObservable(i[property])) {
                set.add(i[property]());
            }
            else
                set.add(i[property])
        });
    }
    else
        list.forEach(function (i) { set.add(i[property]) });
    return set;
}

function UnionArrays(source, incoming, keyProperty) {
    var originalKeys = Project(source, keyProperty);
    var incomingKeys = Project(incoming, keyProperty);

    var keysToRemove = originalKeys.difference(incomingKeys);
    var keysToAdd = incomingKeys.difference(originalKeys);
    var keysToUpdate = originalKeys.intersection(incomingKeys);

    //console.log("Merging Arrays.  [" + keysToRemove.size + "] records will be removed. [" + keysToAdd.size + "] records will be added. [" + keysToUpdate.size + "] records will be updated");

    keysToRemove.forEach(function (key) {
        source.remove(function (item) {
            return item[keyProperty] === key;
        });
    })

    keysToAdd.forEach(function (key) {
        source.push(GetByProperty(incoming, keyProperty, key));
    });

    keysToUpdate.forEach(function (key) {
        var original = GetByProperty(source, keyProperty, key);
        var updated = GetByProperty(incoming, keyProperty, key);
        source.replace(original, updated);
    });
}

Set.prototype.difference = function (setB) {
    var difference = new Set(this);
    for (var elem of setB) {
        difference.delete(elem);
    }
    return difference;
}

Set.prototype.intersection = function (setB) {
    var intersection = new Set();
    for (var elem of setB) {
        if (this.has(elem)) {
            intersection.add(elem);
        }
    }
    return intersection;
}