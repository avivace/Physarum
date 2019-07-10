import Vue from 'vue'
import App from './App.vue'
import "typeface-gentium-basic"

Vue.config.productionTip = false


// Instance Vue and expose it to the global namespace, so we can 
//  touch things from outside, accessing `vm.$children[0]`

global.vm = new Vue({
  render: h => h(App),
}).$mount('#app')