import { DateTime } from 'luxon';

interface LoadHistory {
    timestamp: DateTime;
    load: number;
}
export default LoadHistory;
