var local = undefined;

$(function () {
    local = {
        viewModel: new viewModel()
    };

    ko.applyBindings(local.viewModel);

    LoadData();
    setTimeout(LoadData, 2000);



});

function LoadData() {
    $.getJSON("/data/Dashboard", function (data) {
        var viewModel = local.viewModel;
        UnionArrays(viewModel.builds, data["builds"], "id");
        UnionArrays(viewModel.releases, data["releases"], "requestId");

        SortByKeyDesc(viewModel.builds, "id");
        SortByKeyDesc(viewModel.releases, "requestId");

        setTimeout(LoadData, 2000);
    });
}




var viewModel = function () {
    self = this;
    self.builds = ko.observableArray([]);
    self.releases = ko.observableArray([]);

    self.isNewBuild = function (build) {
        // 10 minutes
        return (new Date(build.finishedDate).getTime() > (new Date().getTime() - 600000));
    };

    self.isNewRelease = function (release) {
        // 10 minutes
        return (new Date(release.finishedTime).getTime() > (new Date().getTime() - 600000));
    }

};


function SortByKeyDesc(list, propertyKey) {
    list.sort(function (left, right) {
        return left[propertyKey] == right[propertyKey] ? 0 :
            left[propertyKey] > right[propertyKey] ? -1 : 1
    });
}

function GetByProperty(list, property, targetValue) {
    for (var i = 0; i < list.length; i++) {
        var item = list[i];
        if (item[property] === targetValue)
            return item;
    }
    return undefined;
}

function Project(list, property) {
    var set = new Set();

    if (ko.isObservable(list)) {
        list().forEach(function (i) { set.add(i[property]) });
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

    console.log("Merging Arrays.  [" + keysToRemove.size + "] records will be removed. [" + keysToAdd.size + "] records will be added. [" + keysToUpdate.size + "] records will be updated");

    keysToRemove.forEach(function (key) {
        source.remove(function (item) {
            return item[keyProperty] === key;
        });            
    })

    keysToAdd.forEach(function (key) {
        source.push(GetByProperty(incoming, keyProperty, key));
    });

    keysToUpdate.forEach(function (key) {
        source.replace(GetByProperty(source, keyProperty, key), GetByProperty(incoming, keyProperty, key))
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