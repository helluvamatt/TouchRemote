const EventBus = new Vue();

const Buttons = Vue.component('buttons', {
    template: '#tpl_buttons',
    data: function () {
        return {
            loaded: false,
            buttons: {}
        };
    },
    computed: {
        buttonsDisplay: function () {
            var $this = this;
            var arr = Object.keys($this.buttons).map(function (key) { return $this.buttons[key]; });
            return Array.prototype.sort.apply(arr, function (a, b) { return a.index - b.index; });
        }
    },
    created: function () {
        var $this = this;
        EventBus.$on('button-updated', function (button) {
            console.log("[Buttons] Got 'button-updated': %o", button);
            $this.buttons[button.Id] = button;
        });
        EventBus.$on('buttons-changed', function (data) {
            console.log("[Buttons] Got 'buttons-changed': %o", data);
            var buttonsDict = {};
            $.each(data, function (index, item) {
                buttonsDict[item.Id] = $.extend({}, item, { index: index });
            });
            $this.buttons = buttonsDict;
            $this.loaded = true;
        });
        EventBus.$on('hub-connected', function () {
            console.log("[Buttons] Got 'hub-connected'");
            $this.loadButtons();
        });
    },
    methods: {
        sendClick: function (id) {
            EventBus.$emit('button-click', id);
        },
        loadButtons: function () {
            this.loaded = false;
            EventBus.$emit('buttons-requested');
        }
    },
    mounted: function () {
        console.log("[Buttons] mounted called");
        this.loadButtons();
    },
    destroyed: function () {
        console.log("[Buttons] destroyed called");
        EventBus.$off('button-updated');
        EventBus.$off('buttons-changed');
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
        { name: 'index', path: '/', component: Buttons },
        { name: 'login', path: '/login', component: Login }
    ]
});

var vm = new Vue({
    el: 'body > #app',
    router: router,
    data: {
        token: '',
        error: null,
        reconnecting: false,
        hubConnected: false
    },
    created: function () {
        var $vm = this;
        var remoteHub = $.connection.remoteHub;
        var loadButtons = false;

        var getButtons = function () {
            if ($vm.hubConnected) {
                remoteHub.server.getButtons($vm.token).done(function (result) {
                    if (result.IsValid) {
                        EventBus.$emit('buttons-changed', result.Data);
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

        // Event from Buttons module: button collection requested, expecting "buttons-changed" event to be fired
        EventBus.$on('buttons-requested', function () {
            console.log("[Main] Got 'buttons-requested'");
            getButtons();
        });

        EventBus.$on('button-click', function (id) {
            if ($vm.hubConnected) {
                remoteHub.server.clickButton($vm.token, id).done(function (result) {
                    if (result.IsValid) {
                        if (!result.Data) {
                            getButtons();
                        }
                    } else {
                        $vm.$router.replace('/login');
                    }
                });
            }
        });

        //void UpdateButton(WebButton webButton);
        remoteHub.client.updateButton = function (button) {
            EventBus.$emit('button-updated', button);
        };

        //void RefreshButtons();
        remoteHub.client.refreshButtons = function () {
            getButtons();
        };

        $.connection.hub.start().done(function () {
            console.log("[Main] hub connected");
            $vm.hubConnected = true;
            EventBus.$emit('hub-connected');
        });
    }
});
