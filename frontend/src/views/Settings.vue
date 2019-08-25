<template>
  <div class="about columns">
    <div class="notification is-primary" id="notification-panel" v-if="notificationVisible">
      <button class="delete" @click="closeNotification"></button>
      Settings udated
    </div>

    <form class="column is-6" @submit="submitForm" @reset="resetForm">
     
      <div class="field is-horizontal">
        <div class="field-label is-normal">
          <label class="label"># of generated messages / min</label>
        </div>
        <div class="field-body">
          <div class="field is-expanded">
            <p class="control">
              <input class="input" type="number" placeholder="100" min="1" max="1000" 
                    pattern="[0-9]" maxlength="5" v-model="formData.generated">
            </p>
          </div>
        </div>
      </div>

      <div class="field is-horizontal">
        <div class="field-label is-normal">
          <label class="label"># of consumed messages / min</label>
        </div>
        <div class="field-body">
          <div class="field is-expanded">
            <p class="control">
              <input class="input" type="number" placeholder="100" min="1" max="1000" 
                    pattern="[0-9]" maxlength="5" v-model="formData.consumed">
            </p>
          </div>
        </div>
      </div>

      <div class="">
        <div class="field is-grouped is-grouped-right">
          <p class="control">
            <button class="button is-light" type="reset">
              Clear
            </button>
          </p>

          <p class="control">
            <button class="button is-primary" type="submit">
              Save
            </button>
          </p>
        </div>
      </div>
    </form>
  </div>
</template>

<style lang="scss" scoped>
#notification-panel {
  bottom: 0;
  position: absolute;
  z-index: 10;
  width: 100%;
}
</style>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { Route } from 'vue-router';

interface FormData {
  generated: number;
  consumed: number;
}


@Component({
  async beforeRouteEnter(to: Route, from: Route, next: any): Promise<any> {
    fetch('/api/settings')
      .then((reponse: any) => reponse.json())
      .then((settings: any) => {
        next((vm: any) => vm.initData(settings.generated, settings.consumed));
      });
  },
})
export default class Settings extends Vue {
  private notificationVisible: boolean = false;
  private notificationTimer: number = 0;

  private formDataDefault!: FormData;
  private formData: FormData = {
    generated: 100,
    consumed: 100,
  };

  public initData(generated: number, consumed: number) {
    this.formDataDefault = { generated, consumed };
    this.formData.generated = generated;
    this.formData.consumed = consumed;
  }

  private closeNotification() {
    this.notificationVisible = false;
  }

  private submitForm(e: Event) {
    fetch('/api/settings', {
      method: 'POST',
      headers: {
          'Content-Type': 'application/json',
      },
      body: JSON.stringify(this.formData),
    }).then((response: Response) => {
      if (response.status === 200) {
        this.notificationVisible = true;
        if (this.notificationTimer){
          clearTimeout(this.notificationTimer);
        }
        this.notificationTimer = setTimeout(() => this.notificationVisible = false, 3 * 1000);
      }
    });
    e.preventDefault();
  }

  private resetForm(e: Event) {
    this.formData.generated = this.formDataDefault.generated;
    this.formData.consumed = this.formDataDefault.consumed;
    e.preventDefault();
  }
}
</script>
