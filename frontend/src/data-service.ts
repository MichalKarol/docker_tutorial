import EventBus from '@/event-bus';
import Node from '@/models/Node';
import { DateTime } from 'luxon';
import LoadHistory from './models/LoadHistory';

let DataServiceInstance: DataService | null = null;

class DataService {
    public static getInstance() {
        if (!DataServiceInstance) {
            return new DataService();
        } else {
            return DataServiceInstance;
        }
    }

    private workers: Node[] = [];
    private queue: Node = {name: 'queue', loadHistory: []};

    private constructor() {
        setInterval(this.updateData.bind(this), 10 * 1000);
        DataServiceInstance = this;
        this.updateData();
    }

    public getWorkers(): Node[] {
        return this.workers;
    }

    public getQueue(): Node {
        return this.queue;
    }

    private async updateData() {
        const queuePromise = fetch('/api/queue');
        const workersPromise = fetch('/api/workers');

        const queueSize = await (await queuePromise).json();
        const queueLoadHistory = this.queue.loadHistory;
        queueLoadHistory.push({load: queueSize, timestamp: DateTime.local()});
        if (queueLoadHistory.length > 20) {
            queueLoadHistory.splice(0, 1);
        }

        const rawWorkers = await (await workersPromise).json();
        this.workers.splice(0, this.workers.length);
        for (const worker of rawWorkers) {
            const workerObject: Node = Object.assign(worker, {
                loadHistory: worker.loadHistory.map((lh: any) => ({
                    load: lh.load,
                    timestamp: DateTime.fromISO(lh.timestamp, { zone: 'UTC' }).toLocal(),
                })
            )});
            const isValid = workerObject.loadHistory.some((lh: LoadHistory) =>
                (lh.timestamp.diff(DateTime.local(), 'minutes').toObject().minutes || 0) > -3);

            if (isValid) {
                this.workers.push(workerObject);
            }
        }
        EventBus.$emit('data-update');
    }
}

export default DataService.getInstance();
