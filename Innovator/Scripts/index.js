function LoginInfo(userLogin, userPassword) {
    this.UserName = userLogin;
    this.UserPassword = userPassword;
}

function TraceInfo(apiToTrace, userId, sessionId) {
    this.ApiToTrace = apiToTrace;
    this.UserId = userId;
    this.SessionId = sessionId;
}

function TraceInfoRequest(tracingId) {
    this.TracingId = tracingId;
}

function ApiCall(data) {
    this.method = data.Method;
    this.request = data.Request;
    this.response = data.Response;
}

function KibanaViewModel() {
    var self = this;
    self.userLogin = ko.observable('');
    self.userPassword = ko.observable('');
    self.sessionId = ko.observable('');
    self.userId = ko.observable('');
    self.userWelcomeMessage = ko.observable('');

    self.apiToTrace = ko.observable('');
    self.tracingId = ko.observable('');
    self.tracingSuccessMessage = ko.observable('');

    self.showLogin = ko.observable(true);
    self.showTracingInfo = ko.observable(false);
    self.showTracingResults = ko.observable(false);

    self.apiCalls = ko.observableArray([]);

    self.existingTracingId = ko.observable('');

    self.login = function () {
        $.ajax("/api/Login", {
            data: JSON.stringify(ko.toJS(new LoginInfo(self.userLogin, self.userPassword))),
            type: "post", contentType: "application/json",
            success: function (data) { self.loginSuccess(data); }
        });
    }

    self.loginSuccess = function (sessionData) {
        self.sessionId(sessionData.SessionId);
        self.userWelcomeMessage('Session Created! Welcome ' + sessionData.DisplayName);
        self.userId(sessionData.UserId);

        self.showLogin(false);
        self.showTracingInfo(true);
    }

    self.trace = function() {
        $.ajax("/api/Trace", {
            data: JSON.stringify(ko.toJS(new TraceInfo(self.apiToTrace, self.userId, self.sessionId))),
            type: "post", contentType: "application/json",
            success: function (data) { self.traceSuccess(data); }
        });
    }

    self.traceSuccess = function (traceData) {
        self.tracingId(traceData.TracingId);
        self.tracingSuccessMessage('Tracing Id Created: ' + traceData.TracingId);

        self.showTracingInfo(false);
        self.showTracingResults(true);
    }

    self.refreshResults = function() {
        $.ajax("/api/TraceResults", {
            data: JSON.stringify(ko.toJS(new TraceInfoRequest(self.tracingId))),
            type: "post", contentType: "application/json",
            success: function (data) { self.traceResultsSuccess(data); }
        });
    }

    self.traceResultsSuccess = function (results) {
        var mappedApiCalls = $.map(results, function (item) { return new ApiCall(item); });
        self.apiCalls(mappedApiCalls);
    }

    self.useExistingTrace = function () {

        self.tracingId(self.existingTracingId());
        self.tracingSuccessMessage('Tracing Id Found: ' + self.tracingId());

        self.showLogin(false);
        self.showTracingInfo(false);
        self.showTracingResults(true);

        self.refreshResults();
    }
}

ko.applyBindings(new KibanaViewModel());