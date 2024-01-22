export interface Tick {
  ask?: number;
  askVolume?: number;
  bid?: number;
  bidVolume?: number;
  date: Date;
  spread: number;
}
