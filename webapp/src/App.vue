<template>
  <div id="app">
    <v-app>
      <v-container fluid>
        <div>
          <span class="brand">Physarum</span>
          <span class="subtitle"
            ><i>&nbsp;&nbsp;Slime mould simulation</i>, September 2019
          </span>
        </div>
        <v-layout row wrap>
          <v-flex md8 sm12>
            <div id="gameContainer" style="width: 80%; height: 600px"></div>
          </v-flex>
          <v-flex md4 sm12>
            Unity status:<span
              style="text-transform: uppercase;font-size: 1.3rem"
            >
              <template v-if="unityStatus==0">
              <v-progress-circular
                indeterminate
                color="black"
                style="height:16px"
              ></v-progress-circular
              >Connecting..
              </template>
              <template v-if="unityStatus==1">
                <check-icon></check-icon> Connected
              </template>
              </span
            >
            <br />
            webgl deploy build:
            <span
              style="font-family: monospace; text-transform: uppercase;font-size: 1.2rem"
            >
              71823SD
            </span>
            <br />
            Simulation status:
            <span style="text-transform: uppercase;font-size: 1.3rem">{{
              status
            }}</span
            ><br />

            <br />
            Time step (t):
            <span style="text-transform: uppercase;font-size: 1.3rem">{{
              time
            }}</span>
            <br />
            N:
            <span style="text-transform: uppercase;font-size: 1.3rem">
              {{ N }}
            </span>
            <br />
            S:
            <span style="text-transform: uppercase;font-size: 1.3rem">
              {{ S }}
            </span>
            <br />
            Mass:
            <span style="text-transform: uppercase;font-size: 1.3rem">
              {{ totalPM }}
            </span>
            <br />

            <br />

            <v-radio-group
              v-model="selectedModel"
              :disabled="status != 'stopped'"
            >
              <v-radio label="Paper Model" value="1"></v-radio>
              <v-radio label="Experimental Model" value="2"></v-radio>
            </v-radio-group>

            <v-btn class="abtn" :disabled="unityStatus == 0" @click="handleStartBtn"
              ><template v-if="status == 'stopped'"
                ><play-icon></play-icon> Start</template
              ><template v-if="status == 'running'"
                ><pause-icon></pause-icon>Pause</template
              >
              <template v-if="status == 'paused'"
                ><play-icon></play-icon>Resume</template
              >
            </v-btn>
            <v-btn class="abtn" :disabled="unityStatus == 0 || (status == 'running')" @click="handleStepBtn">
              <chevron-right-icon></chevron-right-icon> Step
            </v-btn>
            <v-btn @click="handleStopBtn(1)" :disabled="status == 'stopped'"
              ><refresh-ccw-icon></refresh-ccw-icon> &nbsp; Reset
            </v-btn>
            <v-btn @click="handleStopBtn(0)" :disabled="status == 'stopped'"
              ><square-icon size="1x"></square-icon> &nbsp; Stop
            </v-btn>
            <br /><br />
            <v-layout>
              <v-flex xs6>
                <v-select
                  class="numberinput"
                  v-model="selectedMap"
                  :disabled="status != 'stopped'"
                  :items="items"
                  label="Map"
                  outlined
                ></v-select></v-flex
              ><v-flex xs6
                ><v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="defaultcha"
                  type="number"
                  label="Default CHA for N"
                ></v-text-field
              ></v-flex>
            </v-layout>
            <v-layout>
              <v-flex xs6>
                <v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="thpm"
                  type="number"
                  label="thpm"
                ></v-text-field
              ></v-flex>
              <v-flex xs6
                ><v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="defaultpm"
                  type="number"
                  label="Default PM"
                ></v-text-field></v-flex
            ></v-layout>
            <v-layout>
              <v-flex xs6
                ><v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="minagedryout"
                  type="number"
                  label="minAge to dryout"
                ></v-text-field
              ></v-flex>
              <v-flex xs6
                ><v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="cap1"
                  type="number"
                  label="CAP1"
                ></v-text-field
              ></v-flex>
            </v-layout>
            <v-layout>
              <v-flex xs6
                ><v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="cap2"
                  type="number"
                  label="CAP2"
                ></v-text-field
              ></v-flex>
              <v-flex xs6
                ><v-text-field
                  :disabled="status != 'stopped'"
                  class="numberinput"
                  v-model="defaultpms"
                  type="number"
                  label="Default PM for S"
                ></v-text-field
              ></v-flex>
            </v-layout>
          </v-flex>
          <v-snackbar
            v-model="snackbar"
            :bottom="y === 'bottom'"
            :left="x === 'left'"
            :multi-line="mode === 'multi-line'"
            :right="x === 'right'"
            :timeout="timeout"
            :top="y === 'top'"
            :vertical="mode === 'vertical'"
          >
            {{ snackbarText }}
            <v-btn color="pink" flat @click="snackbar = false">
              Close
            </v-btn>
          </v-snackbar>
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
    </v-app>
  </div>
</template>

<script>
import {  ChevronRightIcon,Maximize2Icon,CheckIcon, PlayIcon, PauseIcon, RefreshCcwIcon, SquareIcon } from "vue-feather-icons";
export default {
  components: {
    PlayIcon,
    PauseIcon,
    RefreshCcwIcon,
    SquareIcon,
    CheckIcon,
    Maximize2Icon,
    ChevronRightIcon
  },
  name: "app",
  data: function() {
    return {
      unityStatus: 0,
      status: "stopped",
      greetText: null,
      a: true,
      time: "NAN",
      items: ["Map 1", "Map 2", "Test"],
      selectedModel: "1",
      snackbar: null,
      snackbarText: "Simulation reset",
      selectedMap: "Map 1",
      defaultcha: 0,
      thpm: 0,
      defaultpm: 0,
      minagedryout: 0,
      cap1: 0,
      cap2: 0,
      defaultpms: 0,
      x:0,
      y:0,
      timeout:1000,
      mode:0,
      S: "NAN",
      N: "NAN",
      totalPM: "NAN"
    };
  },
  mounted() {},
  methods: {
    handleStartBtn() {
      gameInstance.SendMessage("GameObject", "startpause")
      if (this.status == "running") {
        this.status = "paused";
      } else {
        this.status = "running";
      }
    },
    handleStopBtn(reset) {
      this.time="NAN"
      if (reset){
        this.timeout = 2000;
        this.snackbarText = 'Simulation stopped and parameters set to default';
        // TODO: reset parameters
      } else {
        this.timeout = 1000;
        this.snackbarText = 'Simulation stopped';
      }
      
      this.status = "stopped";
      this.snackbar = true;
      gameInstance.SendMessage("GameObject", "stop")

    },
    handleStepBtn(){
      //placeholder
    },
    greet(text) {
      this.unityStatus = 1;
    },
    unityUpdate(a, t, S, N, PM){
      if (a == 0){
        // Updating time setp
        this.time = t;
        this.S = S;
        this.N = N;
        this.totalPM = PM;
      }
    }
  }
};
</script>

<style>
/* Vuetify base style is apparently seen as inline, 
so we have to force things with !important */

.numberinput {
  padding-right: 30px;
}
.v-snack__content {
  font-size: 18px !important;
}
.v-btn {
  font-size: 16px !important;
  border-radius: 6px !important;
  min-width: 120px !important;
}
.v-label {
  font-size: 18px !important;
}
.feather {
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
  color: #3d3d5c; /* Darkened Gray Blue (#8C92AC) */
  font-size: 1.2rem;
  background-color: #e8e8ed; /* A really lightened Payne's Gray */
}
.footer {
  position: fixed;
  right: 0;
  bottom: 0;
}
</style>
