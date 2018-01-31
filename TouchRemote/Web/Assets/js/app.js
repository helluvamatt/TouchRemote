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
        return {
            isPressed: false
        };
    },
    computed: {
        fillStyles: function () {
            var styles = {
                backgroundColor: this.Styles.color
            };
            var percent = Number.parseFloat(this.Properties.Value) * 100;
            if (this.Properties.Orientation === 'vertical') {
                styles.height = percent + '%';
            } else {
                styles.width = percent + '%';
            }
            return styles;
        }
    },
    methods: {
        handleDown: function (event) {
            this.isPressed = true;
            this.setValueFromTouch(event);
        },
        handleMove: throttle(function (event) {
            if (this.isPressed) {
                this.setValueFromTouch(event);
            }
        }, 25),
        handleUp: function (event) {
            this.setValueFromTouch(event);
            this.isPressed = false;
        },
        setValueFromTouch: function (event) {
            var $wrapper = $(event.currentTarget);
            var value;
            if (this.Properties.Orientation === 'vertical') {
                var wrapperTop = $wrapper.offset().top;
                var wrapperHeight = $wrapper.height();
                var targetTop = event.targetTouches[0].pageY;
                value = 1 - ((targetTop - wrapperTop) / wrapperHeight);
            } else {
                var wrapperLeft = $wrapper.offset().left;
                var wrapperWidth = $wrapper.width();
                var targetLeft = event.targetTouches[0].pageX;
                value = ((targetLeft - wrapperLeft) / wrapperWidth);
            }
            if (value > 1) value = 1;
            if (value < 0) value = 0;
            this.$emit('control-event', 'value-changed', value);
        }
    }
});

const LabelControl = Vue.component('Label', {
    template: '#tpl_label',
    props: ['Id', 'Type', 'Styles', 'Properties']
});

const TouchpadControl = Vue.component('TouchPad', {
    template: '#tpl_touchpad',
    props: ['Id', 'Type', 'Styles', 'Properties'],
    data: function () {
        return {
            isPressed: false,
            lastX: 0,
            lastY: 0,
            pressStart: 0,
            startX: 0,
            startY: 0
        };
    },
    computed: {
        ctrlStyles: function () {
            return $.extend({}, this.Styles, {
                'border-color': this.Styles.color,
            });
        },
        touchAreaStyles: function () {
            var style = {
                'border-color': this.Styles.color
            };
            if (this.Styles.width === 'auto' && this.Styles.height === 'auto') {
                style['width'] = 'auto';
                style['height'] = 'auto';
            }
            return style;
        },
        buttonAreaStyles: function () {
            return {
                'height': this.Properties.ClickTargetHeight + 'px'
            };
        },
        buttonStyles: function () {
            return {
                'border-color': this.Styles.color
            }
        }
    },
    methods: {
        handleLeftClick: function () {
            this.$emit('control-event', 'mouse-click-left');
        },
        handleMiddleClick: function () {
            this.$emit('control-event', 'mouse-click-middle');
        },
        handleRightClick: function () {
            this.$emit('control-event', 'mouse-click-right');
        },
        handleDown: function (event) {
            this.isPressed = true;
            this.pressStart = Date.now();
            if (event.targetTouches.length >= 2 && this.Properties.AllowScrolling === 'True') {
                var x = 0, y = 0;
                for (var i = 0; i < event.targetTouches.length; i++) {
                    x += event.targetTouches[i].clientX;
                    y += event.targetTouches[i].clientY;
                }
                this.lastX = x / event.targetTouches.length;
                this.lastY = y / event.targetTouches.length;
            } else {
                this.lastX = event.targetTouches[0].clientX;
                this.lastY = event.targetTouches[0].clientY;
            }
            this.startX = this.lastX;
            this.startY = this.lastY;
        },
        handleMove: throttle(function (event) {
            if (this.isPressed) {
                if (event.targetTouches.length >= 2 && this.Properties.AllowScrolling === 'True') {
                    var x = 0, y = 0;
                    for (var i = 0; i < event.targetTouches.length; i++) {
                        x += event.targetTouches[i].clientX;
                        y += event.targetTouches[i].clientY;
                    }
                    x = x / event.targetTouches.length;
                    y = y / event.targetTouches.length;
                    var dY = y - this.lastY;
                    this.$emit('control-event-raw', 'mouse-scroll', Math.round(dY));
                    this.lastX = x;
                    this.lastY = y;
                } else {
                    var dX = event.targetTouches[0].clientX - this.lastX;
                    var dY = event.targetTouches[0].clientY - this.lastY;
                    this.$emit('control-event-raw', 'mouse-move', { dX: dX, dY: dY });
                    this.lastX = event.targetTouches[0].clientX;
                    this.lastY = event.targetTouches[0].clientY;
                }
            }
        }, 25),
        handleUp: function (event) {
            if (this.Properties.TapToClick === 'True' && this.isPressed && event.targetTouches.length == 1 && this.pressStart > 0 && (Date.now() - this.pressStart) < 500) {
                var dX = event.targetTouches[0].clientX - this.startX;
                var dY = event.targetTouches[0].clientY - this.startX;
                if (dX < 20 && dY < 20) {
                    this.$emit('control-event', 'mouse-click-left');
                }
            }
            this.resetDrag();
        },
        resetDrag: function () {
            this.isPressed = false;
            this.pressStart = 0;
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
        EventBus.$on('control-updated-property', function (id, propertyName, propertyValue) {
            $this.controls[id][propertyName] = propertyValue;
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
        handleRawEvent: function (id, eventName, eventData) {
            EventBus.$emit('control-event-raw', id, eventName, eventData);
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

        var sendControlEvent = function (id, name, data) {
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
        };

        // Event from Controls module: control event fired to be sent to server, rate limited to 20 Hz
        var sendControlEventThrottled = throttle(sendControlEvent, 50);
        EventBus.$on('control-event', sendControlEventThrottled);
        EventBus.$on('control-event-raw', sendControlEvent);

        //void UpdateControl(WebButton webButton);
        remoteHub.client.updateControl = function (button) {
            EventBus.$emit('control-updated', button);
        };

        //void RefreshControls();
        remoteHub.client.refreshControls = function () {
            getControls();
        };

        //void UpdateControlProperty(string id, string propertyName, string propertyValue);
        remoteHub.client.updateControlProperty = function (id, propertyName, propertyValue) {
            EventBus.$emit('control-update-property', id, propertyName, propertyValue);
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
