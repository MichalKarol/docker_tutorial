<template>
  <div class="home">
    <div class="columns is-multiline">
      <div class="column is-12">
        <NodeChart :node="queue" type="count" class="node"/>
      </div>

      <div class="column is-4" v-for="node of workers" :key="node.name">
        <NodeChart :node="node" type="load" class="node"/>
      </div>
    </div>
    
    
  </div>
</template>
<style scoped>
.node {
  height: 40vh;
}
</style>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

import DataService from '@/data-service';
import EventBus from '@/event-bus';
import Node from '@/models/Node';
import NodeChart from '@/components/NodeChart.vue';


@Component({
  components: {
    NodeChart,
  },
})
export default class Home extends Vue {
  private queue: Node = DataService.getQueue();
  private workers: Node[] = DataService.getWorkers();

  private mounted() {
    EventBus.$on('data-update', () => {
      this.queue = DataService.getQueue();
      this.workers = DataService.getWorkers();
    });
  }
}
</script>