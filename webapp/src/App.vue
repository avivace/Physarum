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
            Status:
            <span style="text-transform: uppercase;font-size: 1.2rem">{{
              status
            }}</span
            ><br />
            Time step:
            <span style="text-transform: uppercase;font-size: 1.2rem">{{
              time
            }}</span>
            <br /><br /><br />

    <v-radio-group v-model="selectedModel">
      <v-radio
        key="1"
        value="2"
        label="Paper Model"
      ></v-radio>
      <v-radio
        key="2"
        value="2"
        label="Experimental Model"
      ></v-radio>
    </v-radio-group>
    {{selectedModel}}
            <v-btn class="abtn" @click="handleStartBtn"
              ><template v-if="status == 'stopped'"
                ><play-icon></play-icon>
                Start</template
              ><template v-if="status == 'running'"
                ><pause-icon></pause-icon>Pause</template
              >
              <template v-if="status == 'paused'"
                ><play-icon></play-icon>Resume</template
              >
            </v-btn>
            <v-btn @click="handleStopBtn" :disabled='status=="stopped"'
              ><refresh-ccw-icon></refresh-ccw-icon> &nbsp; Reset
            </v-btn>
            <br><br>
            <v-select :disabled="status!='stopped'" :items="items" label="Map" outlined></v-select>
          </v-flex>
        </v-layout>
        <small style="position:fixed;bottom:0"
          >Matteo Coppola, Luca Palazzi, Antonio Vivace
          <i data-feather="github"></i
        ></small>
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
import { PlayIcon, PauseIcon, RefreshCcwIcon } from "vue-feather-icons";
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
      items: ["Map 1", "Map 2", "Test"],
      selectedModel:1
    };
  },
  mounted() {

  },
  methods: {
    handleStartBtn(){
      console.log("handle", this.status)
      if (this.status =="running"){
        this.status="paused"
      } else {
        this.status="running"
      }
    },
    handleStopBtn(){
      this.status="stopped"
    },
    greet(text) {
      this.greetText = text;
    }
  }
};
</script>

<style>
.v-btn{
  font-size: 16px !important;
  border-radius:6px !important;
  min-width: 120px !important;
}
.feather{
  width: 24px;
  height: 24px;
  stroke-width: 2px;
}
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
  font-family: "Barlow", Helvetica, Arial, sans-serif;
  font-weight: 500;
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
