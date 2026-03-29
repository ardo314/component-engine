import type { Component, ComponentProxy } from "@engine/core";

export type ComponentWorker<T extends Component = Component> = {
  readonly component: T;
  create(): ComponentProxy<T>;
};

export function defineComponentWorker<T extends Component>(
  component: T,
  create: () => ComponentProxy<T>,
): ComponentWorker<T> {
  return { component, create };
}
