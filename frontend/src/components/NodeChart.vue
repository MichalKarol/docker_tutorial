<template>
  <div class="node">
    <canvas ref="chart"></canvas>
  </div>
</template>
<style scoped>
.node {
  height: 100%;
}
</style>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import Chart, { ChartPoint, ChartData, ChartDataSets } from 'chart.js';
import { DateTime } from 'luxon';

import EventBus from '@/event-bus';
import LoadHistory from '@/models/LoadHistory';
import Node from '@/models/Node';

@Component
export default class NodeChart extends Vue {
  @Prop() private node!: Node;
  @Prop() private type!: string;

  private chart!: Chart;

  private mounted() {
    const ctx = (this.$refs.chart as HTMLCanvasElement);
    this.chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: this.type === 'load' ? 'Load in %' : '# of messages',
                data: [],
              },
            ],
        },
        options: {
          title: {
            display: true,
            text: this.node.name,
          },
          scales: {
              yAxes: [{
                  ticks: {
                      beginAtZero: true,
                      min: this.type === 'load' ? 0 : undefined,
                      max: this.type === 'load' ? 100 : undefined,
                  },
              }],
          },
          responsive: true,
          maintainAspectRatio: false,
        },
    });

    this.updateChart();
    EventBus.$on('data-update', () => {
      this.updateChart();
    });
  }

  private updateChart() {
    const labels = this.node.loadHistory.map((lh: LoadHistory) => lh.timestamp.toFormat('HH:mm:ss'));
    const data = this.node.loadHistory.map((lh: LoadHistory) => lh.load);

    // Clear data
    this.chart.data!.labels!.length = 0;
    (this.chart.data!.datasets as ChartDataSets[])[0]!.data!.length = 0;

    // Push updated data
    this.chart.data!.labels!.push(...labels);
    (this.chart.data!.datasets as ChartDataSets[])[0]!.data!.push(...data);

    this.chart.update();
  }
}
</script>

<style scoped>
</style>
