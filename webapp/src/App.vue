<template>
  <div id="app">
    <span class="brand">Physarum</span> <br>
    <span class="subtitle"><i>Slime mould simulation</i>, July 2019 </span>
    <div class="footer">
      
       <img src="https://upload.wikimedia.org/wikipedia/commons/1/19/Unity_Technologies_logo.svg" height="30px">&nbsp; <img src="https://upload.wikimedia.org/wikipedia/commons/2/25/WebGL_Logo.svg" height="32px">
    </div>
   
  </div>
</template>

<script>

export default {
  name: 'app'
}
</script>

<style>
.brand {
  font-family: 'Gentium Basic', serif;;
  font-size: 5rem;
  font-weight: 400;
  letter-spacing: -0.25rem;

}

.subtitle {
  font-family: 'Gentium Basic', serif;
  font-size: 1.5rem;
  font-weight: 400;

}
#app {
  font-family: 'Inter', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  margin-top: 5px;
}
.footer{
  position: fixed;

  bottom: 0;

}
</style>
