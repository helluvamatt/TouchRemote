const EventBus = new Vue();

const UnknownControl = Vue.component('Unknown', {
    template: ''
});

const ButtonControl = Vue.component('Button', {
    template: '#tpl_button',
    props: ['Id', 'Type', 'X', 'Y', 'ZIndex', 'Properties'],
    methods: {
        emitClicked: function () {
            this.$emit('control-event', 'click');
        }
    }
});

const SliderControl = Vue.component('Slider', {
    template: '#tpl_slider',
    props: ['Id', 'Type', 'X', 'Y', 'ZIndex', 'Properties'],
    data: function () {
        return {};
    },
    methods: {
        valueChanged: function (e) {
            var value = Number.parseFloat(e.target.value);
            this.$emit('control-event', 'value-changed', value);
        }
    }
});

const Controls = Vue.component('controls', {
    template: '#tpl_controls',
    data: function () {
        return {
            loaded: false,
            controls: {}
        };
    },
    computed: {
        controlsDisplay: function () {
            var $this = this;
            var arr = Object.keys($this.controls).map(function (key) { return $this.controls[key]; });
            return Array.prototype.sort.apply(arr, function (a, b) { return a.index - b.index; });
        }
    },
    created: function () {
        var $this = this;
        EventBus.$on('control-updated', function (control) {
            $this.controls[control.Id] = control;
        });
        EventBus.$on('controls-changed', function (data) {
            var controlsDict = {};
            $.each(data, function (index, item) {
                controlsDict[item.Id] = $.extend({}, item, { index: index });
            });
            $this.controls = controlsDict;
            $this.loaded = true;
        });
        EventBus.$on('hub-connected', function () {
            $this.loadControls();
        });
    },
    methods: {
        handleEvent: function (id, eventName, eventData) {
            EventBus.$emit('control-event', id, eventName, eventData);
        },
        loadControls: function () {
            this.loaded = false;
            EventBus.$emit('controls-requested');
        }
    },
    mounted: function () {
        this.loadControls();
    },
    destroyed: function () {
        EventBus.$off('control-updated');
        EventBus.$off('controls-changed');
        EventBus.$off('hub-connected');
    }
});

const Login = Vue.component('login', {
    template: '#tpl_login',
    data: function () {
        return {
            password: '',
            error: null
        };
    },
    methods: {
        doLogin: function () {
            var $this = this;
            $.ajax({
                method: 'POST',
                url: '/login',
                data: {
                    password: $this.password
                },
                success: function (data, status, jqXHR) {
                    EventBus.$emit('login-success', data.tokenValue);
                    $this.$router.replace('/');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    if (textStatus === "error" && jqXHR.status === 401) {
                        $this.error = "Password is incorrect.";
                    } else if (textStatus === "error" && jqXHR.status === 403) {
                        $this.$router.replace('/');
                    } else {
                        $this.error = "Unable to connect to server. Make sure TouchRemote is running.";
                    }
                }
            });
        }
    }
});

const router = new VueRouter({
    mode: 'history',
    routes: [
        { name: 'index', path: '/', component: Controls },
        { name: 'login', path: '/login', component: Login }
    ],
    scrollBehavior(to, from, savedPosition) {
        return { x: 0, y: 0 };
    }
});

var vm = new Vue({
    el: 'body > #app',
    router: router,
    data: {
        token: '',
        error: null,
        reconnecting: false,
        hubConnected: false,
    },
    created: function () {
        var $vm = this;
        var remoteHub = $.connection.remoteHub;
        var loadControls = false;

        var getControls = function () {
            if ($vm.hubConnected) {
                remoteHub.server.getControls($vm.token).done(function (result) {
                    if (result.IsValid) {
                        EventBus.$emit('controls-changed', result.Data);
                    } else {
                        $vm.$router.replace('/login');
                    }
                });
            }
        };

        $.connection.hub.reconnecting(function () {
            $vm.reconnecting = true;
        });

        $.connection.hub.reconnected(function () {
            $vm.reconnecting = false;
        });

        $.connection.hub.disconnected(function () {
            $vm.reconnecting = false;
            $vm.error = "Lost connection to server. Make sure TouchRemote is running.";
        });

        $.connection.hub.error(function (error) {
            console.error('[SignalR Error] ' + error);
            $vm.error = "Something went wrong. Try restarting TouchRemote.";
        });

        // Event from Login module: we are logged in an have a token
        EventBus.$on('login-success', function (token) {
            $vm.token = token;
        });

        // Event from Controls module: control collection requested, expecting "controls-changed" event to be fired
        EventBus.$on('controls-requested', function () {
            getControls();
        });

        // Event from Controls module: control event fired to be sent to server, rate limited to 10 Hz
        var sendControlEvent = throttle(function (id, name, data) {
            if ($vm.hubConnected) {
                remoteHub.server.processEvent($vm.token, id, name, data).done(function (result) {
                    if (result.IsValid) {
                        if (!result.Data) {
                            getControls();
                        }
                    } else {
                        $vm.$router.replace('/login');
                    }
                });
            }
        }, 50);
        EventBus.$on('control-event', sendControlEvent);

        //void UpdateControl(WebButton webButton);
        remoteHub.client.updateControl = function (button) {
            EventBus.$emit('control-updated', button);
        };

        //void RefreshControls();
        remoteHub.client.refreshControls = function () {
            getControls();
        };

        $.connection.hub.start().done(function () {
            $vm.hubConnected = true;
            EventBus.$emit('hub-connected');
        });
    }
});
