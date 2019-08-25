<template>
  <div id="app">
    <nav class="navbar" role="navigation" aria-label="main navigation">

      <div id="navbarBasicExample" class="navbar-menu is-active">
        <div class="navbar-start">
          <router-link to="/" class="navbar-item">Home</router-link>
          <router-link to="/settings" class="navbar-item">Settings</router-link>
        </div>
      </div>
    </nav>

    <router-view/>
    
  </div>
</template>

<style lang="scss">
@import '../node_modules/bulma/bulma';

$fa-font-path : "../node_modules/@fortawesome/fontawesome-free/webfonts";
@import "../node_modules/@fortawesome/fontawesome-free/scss/fontawesome.scss";
@import "../node_modules/@fortawesome/fontawesome-free/scss/solid.scss";
@import "../node_modules/@fortawesome/fontawesome-free/scss/regular.scss";

html, body, #app {
  height: 100%;
}

#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
}
</style>