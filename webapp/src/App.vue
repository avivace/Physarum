<template>
	<div id="app">
		<v-app>
			<v-container fluid>
				<div>
					<span class="brand">Physarum</span>
					<span class="subtitle"><i>&nbsp;&nbsp;Slime mould simulation</i>, September 2019
					</span>
				</div>
				<v-layout row wrap>
					<v-flex md8 sm12>
						<div id="gameContainer" style="width: 80%; height: 600px"></div>
					</v-flex>
					<v-flex md4 sm12>
						Unity status:<span style="text-transform: uppercase;font-size: 1.3rem">
							<template v-if="unityStatus==0">
								<v-progress-circular indeterminate color="black" style="height:16px"></v-progress-circular>Connecting..
							</template>
							<template v-if="unityStatus==1">
								<check-icon></check-icon> Connected
							</template>
						</span>
						<br />
						webgl deploy build:
						<span style="font-family: monospace; text-transform: uppercase;font-size: 1.2rem">
							71823SD
						</span>
						<br />
						Simulation status:
						<span style="text-transform: uppercase;font-size: 1.3rem">{{
							status
							}}</span><br />
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
						<v-radio-group v-model="selectedModel" :disabled="unityStatus == 0 || status != 'stopped'" @change="changeModel"label="Simulation Model:" >
							<v-radio label="Paper" :value="0"></v-radio>
							<v-radio label="Experimental" :value="1"></v-radio>
						</v-radio-group>
						<v-slider v-model="fps" label="Target simulation t/s" style="width: 500px" min="1" max="60" @change="changeSpeed"><template v-slot:append>
								<v-text-field v-model="fps" class="mt-0 pt-0" hide-details single-line type="number" style="width: 40px"></v-text-field>
							</template></v-slider>
						<v-btn class="abtn" :disabled="unityStatus == 0" @click="handleStartBtn"><template v-if="status == 'stopped'">
								<play-icon></play-icon> Start
							</template><template v-if="status == 'running'">
								<pause-icon></pause-icon>Pause
							</template>
							<template v-if="status == 'paused'">
								<play-icon></play-icon>Resume
							</template>
						</v-btn>
						<v-btn class="abtn" :disabled="unityStatus == 0 || (status == 'running')" @click="handleStepBtn">
							<chevron-right-icon></chevron-right-icon> Step
						</v-btn>
						<v-btn @click="handleStopBtn(1)" :disabled="status == 'stopped'">
							<refresh-ccw-icon></refresh-ccw-icon> &nbsp; Reset
						</v-btn>
						<v-btn @click="handleStopBtn(0)" :disabled="status == 'stopped'">
							<square-icon size="1x"></square-icon> &nbsp; Stop
						</v-btn>
						<br /><br />
						<v-layout>
							<v-flex xs6>
								<v-select class="numberinput" v-model="selectedMap" :disabled="unityStatus == 0 || status != 'stopped'" :items="mapItems" item-text="fileName" item-value="mapIndex" label="Map" outlined @change="changeMap"></v-select>
							</v-flex>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="defaultCHAForN" type="number" label="Default CHA for N"></v-text-field>
							</v-flex>
						</v-layout>
						<v-layout>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="ThPM" @change="updateParameters" type="number" label="thpm"></v-text-field>
							</v-flex>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="defaultPMForS" @change="updateParameters" type="number" label="Default PM for S"></v-text-field>
							</v-flex>
						</v-layout>
						<v-layout>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="minAgeToDryOut" @change="updateParameters" type="number" label="minAge to dryout"></v-text-field>
							</v-flex>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="CAP1" @change="updateParameters" type="number" label="CAP1"></v-text-field>
							</v-flex>
						</v-layout>
						<v-layout>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="CAP2" @change="updateParameters" type="number" label="CAP2"></v-text-field>
							</v-flex>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="PMP1" @change="updateParameters" type="number" label="PMP1"></v-text-field>
							</v-flex>
						</v-layout>
												<v-layout>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="CON" @change="updateParameters" type="number" label="CON"></v-text-field>
							</v-flex>
							<v-flex xs6>
								<v-text-field :disabled="unityStatus == 0 || status != 'stopped'" class="numberinput" v-model="PMP2" @change="updateParameters" type="number" label="PMP2"></v-text-field>
							</v-flex>
						</v-layout>
					</v-flex>
					<v-snackbar v-model="snackbar" :bottom="y === 'bottom'" :left="x === 'left'" :multi-line="mode === 'multi-line'" :right="x === 'right'" :timeout="timeout" :top="y === 'top'" :vertical="mode === 'vertical'">
						{{ snackbarText }}
						<v-btn color="pink" flat @click="snackbar = false">
							Close
						</v-btn>
					</v-snackbar>
				</v-layout>
				<small style="position:fixed;bottom:0">Matteo Coppola, Luca Palazzi, Antonio Vivace
					<i data-feather="github"></i></small>
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
import { ChevronRightIcon, Maximize2Icon, CheckIcon, PlayIcon, PauseIcon, RefreshCcwIcon, SquareIcon } from "vue-feather-icons";
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
			// Map items
			mapItems: [],
			items: ["Map 1", "Map 2", "Test"],
			selectedModel: 1,
			snackbar: null,
			snackbarText: "Simulation reset",
			selectedMap: {},
			x: 0,
			y: 0,
			timeout: 1000,
			mode: 0,
			S: "NAN",
			N: "NAN",
			totalPM: "NAN",
			fps: 60,
			defaultPMForS:-1,
			defaultCHAForN: -1,
			CON: -1,
			CAP1: -1,
			CAP2: -1,
			ThPM: -1,
			minAgeToDryOut: -1,
			PMP1: -1,
			PMP2: -1,
		};
	},
	mounted() {},
	methods: {
		changeModel() {
			gameInstance.SendMessage("GameObject", "selectSimulationMode", this.selectedModel)
			console.log("GameObject", "selectSimulationMode", this.selectedModel)
		},
		handleStartBtn() {
			if (this.status == "stopped") {
				this.updateParameters();
			}
			gameInstance.SendMessage("GameObject", "startpause")
			if (this.status == "running") {
				this.status = "paused";
			} else {
				this.status = "running";
			}
		},
		handleStopBtn(reset) {
			this.time = "NAN"
			this.updateParameters();
			gameInstance.SendMessage("GameObject", "stop")
			if (reset) {
				this.timeout = 2000;
				this.snackbarText = 'Simulation stopped and parameters set to default';
				gameInstance.SendMessage("GameObject", "selectMap", this.selectedMap)
			} else {
				this.timeout = 1000;
				this.snackbarText = 'Simulation stopped';
			}

			this.status = "stopped";
			this.snackbar = true;

		},
		updateParameters(){
			// VUE -> UNITY
			let values = [this.defaultPMForS,
				this.defaultCHAForN,
				this.CON,
				this.CAP1,
				this.CAP2,
				this.ThPM,
				this.minAgeToDryOut,
				this.PMP1,
				this.PMP2];
			gameInstance.SendMessage("GameObject", "setParameters", values.join(';'))
			console.log("GameObject", "setParameters", values.join(';'))
			
		},
		unityParamUpdate(defaultPMForS,
			defaultCHAForN,
			CON,
			CAP1,
			CAP2,
			ThPM,
			minAgeToDryOut,
			PMP1,
			PMP2){
			this.defaultPMForS = defaultPMForS;
			this.defaultCHAForN = defaultCHAForN;
			this.CON = CON;
			this.CAP1 = CAP1;
			this.CAP2 = CAP2;
			this.ThPM = ThPM;
			this.minAgeToDryOut = minAgeToDryOut;
			this.PMP1 = PMP1;
			this.PMP2 = PMP2;
			// VUE <- UNITY
		},
		changeSpeed() {
			gameInstance.SendMessage("GameObject", "changeFrameRate", this.fps)
		},
		handleStepBtn() {
			gameInstance.SendMessage("GameObject", "simulationStep")
		},
		greet(text) {
			this.unityStatus = 1;
			this.changeModel();
		},
		unityUpdate(a, t, S, N, PM) {
			if (a == 0) {
				// Updating time setp
				this.time = t;
				this.S = S;
				this.N = N;
				this.totalPM = PM;
			}
		},
		changeMap(mapIndex) {
			console.log("Selecting map", this.selectedMap)
			gameInstance.SendMessage("GameObject", "selectMap", this.selectedMap)
		},
		updateMaps(rawMapListArray, currentMapIndex) {
			console.log(rawMapListArray, currentMapIndex)

			// Deserialize the arriving list of map file names
			let mapListArray = rawMapListArray.split(",");

			let mapItems = this.mapItems;

			mapListArray.forEach(function(name, index) {
				console.log(name, index)
				let mapElement = {
					fileName: name,
					mapIndex: index
				}
				mapItems.push(mapElement)
			})

			this.selectedMap = currentMapIndex;

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
	color: #3d3d5c;
	/* Darkened Gray Blue (#8C92AC) */
	font-size: 1.2rem;
	background-color: #e8e8ed;
	/* A really lightened Payne's Gray */
}

.footer {
	position: fixed;
	right: 0;
	bottom: 0;
}
</style>