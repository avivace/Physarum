<template>
  <div id="app">
    <span class="brand">Physarum</span> <br />
    <span class="subtitle"><i>Slime mould simulation</i>, July 2019 </span>
    <div class="footer">
      <img
        src="./assets/Unity_Technologies_logo.svg"
        height="34px"
      />&nbsp;
      <img
        src="./assets/WebGL_Logo.svg"
        height="38px"
      />
    </div>
    <br /><br /><br />
    {{ greetText }}
  </div>
</template>

<script>
export default {
  name: "app",
  data: function() {
    return {
      greetText: null
    };
  },
  mounted() {},
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
  font-size: 1.5rem;
  font-weight: 400;
  letter-spacing: -0.02rem;
}
#app {
  font-family: "Inter", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  margin-top: 5px;
  font-size: 1.5rem;
}
.footer {
  position: fixed;
  right: 0;
  bottom: 0;
}
</style>
