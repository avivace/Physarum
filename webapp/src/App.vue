<template>
  <div id="app">
    <v-app>
      <v-container fluid>
        <div>
          <span class="brand">Physarum</span>
          <span class="subtitle"
            ><i>&nbsp;&nbsp;Slime mould simulation</i>, July 2019 
          </span>
        </div>
        <v-layout row wrap>
          <v-flex md9 sm12>
            webgl container
          </v-flex>
          <v-flex md3 sm12>
            Status: <span style="text-transform: uppercase;font-size: 1.2rem">{{status}}</span><br>
            Time step: <span style="text-transform: uppercase;font-size: 1.2rem">{{time}}</span>
            <br><br><br>

            <v-btn @click="a=false"><template v-if="a"><play-icon></play-icon></template><template v-if="a==false"><pause-icon></pause-icon></template> &nbsp;Start simulation </v-btn>
            <v-btn disabled><refresh-ccw-icon></refresh-ccw-icon> &nbsp; Reset </v-btn>   
                    <v-select
          :items="items"
          label="Map"
          outlined
        ></v-select> 
          </v-flex>
        </v-layout>
        <small style="position:fixed;bottom:0"
          >Matteo Coppola, Luca Palazzi, Antonio Vivace <i data-feather="github"></i></small
        >
      </v-container>
      <div class="footer">
        <img src="./assets/Unity_Technologies_logo.svg" height="34px" />&nbsp;
        <img src="./assets/WebGL_Logo.svg" height="38px" />
      </div>
      <br /><br /><br />
      {{ greetText }}
    </v-app>
  </div>
</template>

<script>
import { PlayIcon, PauseIcon,RefreshCcwIcon } from 'vue-feather-icons'
export default {
    components: {
    PlayIcon,
    PauseIcon,
    RefreshCcwIcon
  },
  name: "app",
  data: function() {
    return {
      status: "stopped",
      greetText: null,
      a: true,
      time: 50,
      items: ["Map 1", "Map 2", "Test"]
    };
  },
  mounted() {
    feather.replace();
  },
  methods: {
    greet(text) {
      this.greetText = text;
    }
  }
};
</script>

<style>
.brand {
  font-family: "Gentium Basic", serif;
  font-size: 5.5rem;
  font-weight: 400;
  letter-spacing: -0.25rem;
  background-color: #111;
  -webkit-background-clip: text;
  -moz-background-clip: text;
  background-clip: text;
  color: transparent;
  text-shadow: rgba(255, 255, 255, 0.4) 0px 3px 3px;
}

.subtitle {
  font-family: "Gentium Basic", serif;
  font-size: 1.75rem;
  font-weight: 400;
  letter-spacing: -0.02rem;
}
#app {
  font-family: "Inter", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  font-size: 1.5rem;
}
.footer {
  position: fixed;
  right: 0;
  bottom: 0;
}
</style>
