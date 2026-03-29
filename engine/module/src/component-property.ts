export type ComponentProperty<T> = {
  get(): Promise<T>;
  set(value: T): Promise<void>;
};
