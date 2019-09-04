(function(t){function e(e){for(var n,l,r=e[0],o=e[1],u=e[2],p=0,d=[];p<r.length;p++)l=r[p],s[l]&&d.push(s[l][0]),s[l]=0;for(n in o)Object.prototype.hasOwnProperty.call(o,n)&&(t[n]=o[n]);c&&c(e);while(d.length)d.shift()();return i.push.apply(i,u||[]),a()}function a(){for(var t,e=0;e<i.length;e++){for(var a=i[e],n=!0,r=1;r<a.length;r++){var o=a[r];0!==s[o]&&(n=!1)}n&&(i.splice(e--,1),t=l(l.s=a[0]))}return t}var n={},s={app:0},i=[];function l(e){if(n[e])return n[e].exports;var a=n[e]={i:e,l:!1,exports:{}};return t[e].call(a.exports,a,a.exports,l),a.l=!0,a.exports}l.m=t,l.c=n,l.d=function(t,e,a){l.o(t,e)||Object.defineProperty(t,e,{enumerable:!0,get:a})},l.r=function(t){"undefined"!==typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(t,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(t,"__esModule",{value:!0})},l.t=function(t,e){if(1&e&&(t=l(t)),8&e)return t;if(4&e&&"object"===typeof t&&t&&t.__esModule)return t;var a=Object.create(null);if(l.r(a),Object.defineProperty(a,"default",{enumerable:!0,value:t}),2&e&&"string"!=typeof t)for(var n in t)l.d(a,n,function(e){return t[e]}.bind(null,n));return a},l.n=function(t){var e=t&&t.__esModule?function(){return t["default"]}:function(){return t};return l.d(e,"a",e),e},l.o=function(t,e){return Object.prototype.hasOwnProperty.call(t,e)},l.p="/Physarum/";var r=window["webpackJsonp"]=window["webpackJsonp"]||[],o=r.push.bind(r);r.push=e,r=r.slice();for(var u=0;u<r.length;u++)e(r[u]);var c=o;i.push([0,"chunk-vendors"]),a()})({0:function(t,e,a){t.exports=a("56d7")},"034f":function(t,e,a){"use strict";var n=a("64a9"),s=a.n(n);s.a},"1ad1":function(t,e,a){t.exports=a.p+"img/Unity_Technologies_logo.e5067f78.svg"},"3dfd":function(t,e,a){"use strict";var n=function(){var t=this,e=t.$createElement,n=t._self._c||e;return n("div",{attrs:{id:"app"}},[n("v-app",[n("v-container",{attrs:{fluid:""}},[n("div",[n("span",{staticClass:"brand"},[t._v("Physarum")]),n("span",{staticClass:"subtitle"},[n("i",[t._v("  Slime mould simulation")]),t._v(", September 2019\n\t\t\t\t")])]),n("v-layout",{attrs:{row:"",wrap:""}},[n("v-flex",{attrs:{md8:"",sm12:""}},[n("div",{staticStyle:{width:"80%",height:"600px"},attrs:{id:"gameContainer"}})]),n("v-flex",{attrs:{md4:"",sm12:""}},[t._v("\n\t\t\t\t\tUnity status:"),n("span",{staticStyle:{"text-transform":"uppercase","font-size":"1.3rem"}},[0==t.unityStatus?[n("v-progress-circular",{staticStyle:{height:"16px"},attrs:{indeterminate:"",color:"black"}}),t._v("Connecting..\n\t\t\t\t\t\t")]:t._e(),1==t.unityStatus?[n("check-icon"),t._v(" Connected\n\t\t\t\t\t\t")]:t._e()],2),n("br"),t._v("\n\t\t\t\t\twebgl deploy build:\n\t\t\t\t\t"),n("span",{staticStyle:{"font-family":"monospace","text-transform":"uppercase","font-size":"1.2rem"}},[t._v("\n\t\t\t\t\t\t71823SD\n\t\t\t\t\t")]),n("br"),t._v("\n\t\t\t\t\tSimulation status:\n\t\t\t\t\t"),n("span",{staticStyle:{"text-transform":"uppercase","font-size":"1.3rem"}},[t._v(t._s(t.status))]),n("br"),n("br"),t._v("\n\t\t\t\t\tTime step (t):\n\t\t\t\t\t"),n("span",{staticStyle:{"text-transform":"uppercase","font-size":"1.3rem"}},[t._v(t._s(t.time))]),n("br"),t._v("\n\t\t\t\t\tN:\n\t\t\t\t\t"),n("span",{staticStyle:{"text-transform":"uppercase","font-size":"1.3rem"}},[t._v("\n\t\t\t\t\t\t"+t._s(t.N)+"\n\t\t\t\t\t")]),n("br"),t._v("\n\t\t\t\t\tS:\n\t\t\t\t\t"),n("span",{staticStyle:{"text-transform":"uppercase","font-size":"1.3rem"}},[t._v("\n\t\t\t\t\t\t"+t._s(t.S)+"\n\t\t\t\t\t")]),n("br"),t._v("\n\t\t\t\t\tMass:\n\t\t\t\t\t"),n("span",{staticStyle:{"text-transform":"uppercase","font-size":"1.3rem"}},[t._v("\n\t\t\t\t\t\t"+t._s(t.totalPM)+"\n\t\t\t\t\t")]),n("br"),n("br"),n("v-radio-group",{attrs:{disabled:0==t.unityStatus||"stopped"!=t.status},on:{change:t.changeModel},model:{value:t.selectedModel,callback:function(e){t.selectedModel=e},expression:"selectedModel"}},[n("v-radio",{attrs:{label:"Paper Model",value:0}}),n("v-radio",{attrs:{label:"Experimental Model",value:1}})],1),n("v-slider",{staticStyle:{width:"500px"},attrs:{label:"Simulation Speed ",min:"1",max:"60"},on:{change:t.changeSpeed},scopedSlots:t._u([{key:"append",fn:function(){return[n("v-text-field",{staticClass:"mt-0 pt-0",staticStyle:{width:"40px"},attrs:{"hide-details":"","single-line":"",type:"number"},model:{value:t.fps,callback:function(e){t.fps=e},expression:"fps"}})]},proxy:!0}]),model:{value:t.fps,callback:function(e){t.fps=e},expression:"fps"}}),n("v-btn",{staticClass:"abtn",attrs:{disabled:0==t.unityStatus},on:{click:t.handleStartBtn}},["stopped"==t.status?[n("play-icon"),t._v(" Start\n\t\t\t\t\t\t")]:t._e(),"running"==t.status?[n("pause-icon"),t._v("Pause\n\t\t\t\t\t\t")]:t._e(),"paused"==t.status?[n("play-icon"),t._v("Resume\n\t\t\t\t\t\t")]:t._e()],2),n("v-btn",{staticClass:"abtn",attrs:{disabled:0==t.unityStatus||"running"==t.status},on:{click:t.handleStepBtn}},[n("chevron-right-icon"),t._v(" Step\n\t\t\t\t\t")],1),n("v-btn",{attrs:{disabled:"stopped"==t.status},on:{click:function(e){return t.handleStopBtn(1)}}},[n("refresh-ccw-icon"),t._v("   Reset\n\t\t\t\t\t")],1),n("v-btn",{attrs:{disabled:"stopped"==t.status},on:{click:function(e){return t.handleStopBtn(0)}}},[n("square-icon",{attrs:{size:"1x"}}),t._v("   Stop\n\t\t\t\t\t")],1),n("br"),n("br"),n("v-layout",[n("v-flex",{attrs:{xs6:""}},[n("v-select",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,items:t.mapItems,"item-text":"fileName","item-value":"mapIndex",label:"Map",outlined:""},on:{change:t.changeMap},model:{value:t.selectedMap,callback:function(e){t.selectedMap=e},expression:"selectedMap"}})],1),n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"Default CHA for N"},model:{value:t.defaultcha,callback:function(e){t.defaultcha=e},expression:"defaultcha"}})],1)],1),n("v-layout",[n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"thpm"},model:{value:t.thpm,callback:function(e){t.thpm=e},expression:"thpm"}})],1),n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"Default PM"},model:{value:t.defaultpm,callback:function(e){t.defaultpm=e},expression:"defaultpm"}})],1)],1),n("v-layout",[n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"minAge to dryout"},model:{value:t.minagedryout,callback:function(e){t.minagedryout=e},expression:"minagedryout"}})],1),n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"CAP1"},model:{value:t.cap1,callback:function(e){t.cap1=e},expression:"cap1"}})],1)],1),n("v-layout",[n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"CAP2"},model:{value:t.cap2,callback:function(e){t.cap2=e},expression:"cap2"}})],1),n("v-flex",{attrs:{xs6:""}},[n("v-text-field",{staticClass:"numberinput",attrs:{disabled:0==t.unityStatus||"running"==t.status,type:"number",label:"Default PM for S"},model:{value:t.defaultpms,callback:function(e){t.defaultpms=e},expression:"defaultpms"}})],1)],1)],1),n("v-snackbar",{attrs:{bottom:"bottom"===t.y,left:"left"===t.x,"multi-line":"multi-line"===t.mode,right:"right"===t.x,timeout:t.timeout,top:"top"===t.y,vertical:"vertical"===t.mode},model:{value:t.snackbar,callback:function(e){t.snackbar=e},expression:"snackbar"}},[t._v("\n\t\t\t\t\t"+t._s(t.snackbarText)+"\n\t\t\t\t\t"),n("v-btn",{attrs:{color:"pink",flat:""},on:{click:function(e){t.snackbar=!1}}},[t._v("\n\t\t\t\t\t\tClose\n\t\t\t\t\t")])],1)],1),n("small",{staticStyle:{position:"fixed",bottom:"0"}},[t._v("Matteo Coppola, Luca Palazzi, Antonio Vivace\n\t\t\t\t"),n("i",{attrs:{"data-feather":"github"}})])],1),n("div",{staticClass:"footer"},[n("img",{attrs:{src:a("1ad1"),height:"34px"}}),t._v(" \n\t\t\t"),n("img",{attrs:{src:a("e0c5"),height:"38px"}})]),n("br"),n("br"),n("br")],1)],1)},s=[],i=(a("ac6a"),a("28a5"),a("0a35")),l={components:{PlayIcon:i["e"],PauseIcon:i["d"],RefreshCcwIcon:i["f"],SquareIcon:i["g"],CheckIcon:i["a"],Maximize2Icon:i["c"],ChevronRightIcon:i["b"]},name:"app",data:function(){return{unityStatus:0,status:"stopped",greetText:null,a:!0,time:"NAN",mapItems:[],items:["Map 1","Map 2","Test"],selectedModel:1,snackbar:null,snackbarText:"Simulation reset",selectedMap:{},defaultcha:0,thpm:0,defaultpm:0,minagedryout:0,cap1:0,cap2:0,defaultpms:0,x:0,y:0,timeout:1e3,mode:0,S:"NAN",N:"NAN",totalPM:"NAN",fps:60}},mounted:function(){},methods:{changeModel:function(){gameInstance.SendMessage("GameObject","selectSimulationMode",this.selectedModel),console.log("GameObject","selectSimulationMode",this.selectedModel)},handleStartBtn:function(){gameInstance.SendMessage("GameObject","startpause"),"running"==this.status?this.status="paused":this.status="running"},handleStopBtn:function(t){this.time="NAN",t?(this.timeout=2e3,this.snackbarText="Simulation stopped and parameters set to default"):(this.timeout=1e3,this.snackbarText="Simulation stopped"),this.status="stopped",this.snackbar=!0,gameInstance.SendMessage("GameObject","stop")},changeSpeed:function(){gameInstance.SendMessage("GameObject","changeFrameRate",this.fps)},handleStepBtn:function(){gameInstance.SendMessage("GameObject","simulationStep")},greet:function(t){this.unityStatus=1,this.changeModel()},unityUpdate:function(t,e,a,n,s){0==t&&(this.time=e,this.S=a,this.N=n,this.totalPM=s)},changeMap:function(t){console.log("Selecting map",this.selectedMap),gameInstance.SendMessage("GameObject","selectMap",this.selectedMap)},updateMaps:function(t,e){console.log(t,e);var a=t.split(","),n=this.mapItems;a.forEach(function(t,e){console.log(t,e);var a={fileName:t,mapIndex:e};n.push(a)}),this.selectedMap=e}}},r=l,o=(a("034f"),a("2877")),u=Object(o["a"])(r,n,s,!1,null,null,null);e["a"]=u.exports},"56d7":function(t,e,a){"use strict";a.r(e),function(t){a("cadf"),a("551c"),a("f751"),a("097d");var e=a("2b0e"),n=a("3dfd"),s=(a("f8d6"),a("ce5b")),i=a.n(s);a("d244"),a("bf40");e["default"].use(i.a),e["default"].config.productionTip=!1,t.vm=new e["default"]({render:function(t){return t(n["a"])}}).$mount("#app")}.call(this,a("c8ba"))},"64a9":function(t,e,a){},e0c5:function(t,e,a){t.exports=a.p+"img/WebGL_Logo.496850a0.svg"}});
//# sourceMappingURL=app.af82fdbd.js.map