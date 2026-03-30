import type { Component, ComponentProxy } from "@engine/core";

export type ComponentWorker<C extends Component = Component> = {
  readonly component: C;
  create(): ComponentProxy<C>;
};

export function defineComponentWorker<C extends Component>(
  component: C,
  create: () => ComponentProxy<C>,
): ComponentWorker<C> {
  return { component, create };
}
