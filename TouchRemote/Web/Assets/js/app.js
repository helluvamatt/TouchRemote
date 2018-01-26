const Enum = {
    hasFlag: function (value, flags) {
        return (value & flags) === flags;
    }
};

const AuthState = {
    Authenticated: 0,
    NoPassword: 1,
    ExceedsMaxConnections: 2,
    IPNotAllowed: 4,
    InvalidConnection: 8
};

const EventBus = new Vue();

const UnknownControl = Vue.component('Unknown', {
    template: ''
});

const ButtonControl = Vue.component('Button', {
    template: '#tpl_button',
    data: function () {
        return {
            isMouseOver: false
        };
    },
    props: ['Id', 'Type', 'Styles', 'Properties'],
    computed: {
        realStyles: function () {
            var hoverStyles = {
                'backgroundColor': this.Properties.ActiveBackgroundColor,
                'color': this.Properties.ActiveColor
            };
            return $.extend({}, this.Styles, this.isMouseOver ? hoverStyles : {});
        }
    },
    methods: {
        emitClicked: function () {
            this.$emit('control-event', 'click');
        }
    }
});

const SliderControl = Vue.component('Slider', {
    template: '#tpl_slider',
    props: ['Id', 'Type', 'Styles', 'Properties'],
    data: function () {
        return {};
    },
    computed: {
        wrapperStyles: function () {
            if (this.Styles.width !== 'auto' && this.Styles.height !== 'auto') {
                if (this.Properties.Orientation === 'vertical') {
                    var height = Number.parseFloat(this.Styles.height) - 10;
                    return { 'height': height + 'px' };
                } else {
                    var width = Number.parseFloat(this.Styles.width) - 10;
                    return { 'width': width + 'px' };
                }
            }
            return {};
        },
        inputRangeStyles: function () {
            if (this.Styles.width !== 'auto' && this.Styles.height !== 'auto') {
                var length = Number.parseFloat(this.Properties.Orientation === 'vertical' ? this.Styles.height : this.Styles.width) - 10;
                var styles = { 'width': length + 'px' };
                if (this.Properties.Orientation === 'vertical') {
                    var half = length / 2;
                    styles['transformOrigin'] = half + 'px ' + half + 'px';
                }
                return styles;
            }
            return {};
        }
    },
    methods: {
        valueChanged: function (e) {
            var value = Number.parseFloat(e.target.value);
            this.$emit('control-event', 'value-changed', value);
        }
    }
});

const LabelControl = Vue.component('Label', {
    template: '#tpl_label',
    props: ['Id', 'Type', 'Styles', 'Properties']
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
    props: ['authError'],
    data: function () {
        return {
            password: '',
            error: this.authError
        };
    },
    methods: {
        doLogin: function () {
            var $vm = this;
            EventBus.$emit('login', this.password, function (errorMessage) {
                $vm.error = errorMessage;
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
        resizeHandler: null,
        authError: null
    },
    created: function () {
        var $vm = this;
        var remoteHub = $.connection.remoteHub;
        var loadControls = false;

        this.resizeHandler = throttle(function () {
            if ($vm.hubConnected) {
                var width = document.documentElement.clientWidth;
                var height = document.documentElement.clientHeight;
                remoteHub.server.setClientSize($vm.token, width, height).done(function (result) {
                    if (result.AuthState != AuthState.Authenticated) {
                        if (Enum.hasFlag(result.AuthState, AuthState.ExceedsMaxConnections) || Enum.hasFlag(result.AuthState, AuthState.IPNotAllowed))
                            $vm.authError = "You are not allowed to connect at this time.";
                        else
                            $vm.authError = "Please login.";
                        $vm.$router.replace('/login');
                    }
                });
            }
        }, 50);

        window.addEventListener('resize', this.resizeHandler);

        var getControls = function () {
            if ($vm.hubConnected) {
                remoteHub.server.getControls($vm.token).done(function (result) {
                    if (result.AuthState == AuthState.Authenticated) {
                        EventBus.$emit('controls-changed', result.Data);
                    } else {
                        if (Enum.hasFlag(result.AuthState, AuthState.ExceedsMaxConnections) || Enum.hasFlag(result.AuthState, AuthState.IPNotAllowed))
                            $vm.authError = "You are not allowed to connect at this time.";
                        else
                            $vm.authError = "Please login.";
                        $vm.$router.replace('/login');
                    }
                });
            }
        };

        window.addEventListener('resize', this.handleResize);

        $.connection.hub.reconnecting(function () {
            $vm.reconnecting = true;
        });

        $.connection.hub.reconnected(function () {
            $vm.reconnecting = false;
            $vm.resizeHandler();
        });

        $.connection.hub.disconnected(function () {
            $vm.reconnecting = false;
            $vm.error = "Lost connection to server. Make sure TouchRemote is running.";
        });

        $.connection.hub.error(function (error) {
            console.error('[SignalR Error] ' + error);
            $vm.error = "Something went wrong. Try restarting TouchRemote.";
        });

        // Event from Controls module: control collection requested, expecting "controls-changed" event to be fired
        EventBus.$on('controls-requested', function () {
            getControls();
        });

        // Event from Login module: login requested to be performed over SignalR
        EventBus.$on('login', function (password, errorCallback) {
            if ($vm.hubConnected) {
                remoteHub.server.login(password).done(function (result) {
                    if (result.AuthState == AuthState.Authenticated) {
                        // result.Data is the token
                        $vm.token = result.Data;
                        $vm.$router.replace('/');
                    } else {
                        if (Enum.hasFlag(result.AuthState, AuthState.NoPassword))
                            errorCallback("Password is incorrect.");
                        else
                            errorCallback("You are not allowed to connect at this time.")
                    }
                });
            }
        });

        // Event from Controls module: control event fired to be sent to server, rate limited to 10 Hz
        var sendControlEvent = throttle(function (id, name, data) {
            if ($vm.hubConnected) {
                remoteHub.server.processEvent($vm.token, id, name, data).done(function (result) {
                    if (result.AuthState == AuthState.Authenticated) {
                        if (!result.Data) {
                            getControls();
                        }
                    } else {
                        if (Enum.hasFlag(result.AuthState, AuthState.ExceedsMaxConnections) || Enum.hasFlag(result.AuthState, AuthState.IPNotAllowed))
                            $vm.authError = "You are not allowed to connect at this time.";
                        else
                            $vm.authError = "Please login.";
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
            $vm.resizeHandler();
        });
    },
    beforeDestroy: function () {
        window.removeEventListener('resize', this.handleResize)
    }
});
