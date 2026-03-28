import type { NatsConnection } from "nats";
import { StringCodec } from "nats";
import type { ComponentProxy, EntityId } from "@engine/core";
import { Subjects, Component } from "@engine/core";

const sc = StringCodec();

export class Entity {
  constructor(
    private readonly nc: NatsConnection,
    public readonly id: EntityId,
  ) {}

  async addComponent<T extends Component>(
    component: T,
  ): Promise<ComponentProxy<T>> {
    await this.nc.request(
      Subjects.addComponent,
      sc.encode(
        JSON.stringify({ entityId: this.id, componentId: component.id }),
      ),
    );
    return null as ComponentProxy<T>;
  }

  async removeComponent<T extends Component>(component: T): Promise<void> {
    await this.nc.request(
      Subjects.removeComponent,
      sc.encode(
        JSON.stringify({ entityId: this.id, componentId: component.id }),
      ),
    );
  }

  async hasComponent<T extends Component>(component: T): Promise<boolean> {
    const reply = await this.nc.request(
      Subjects.hasComponent,
      sc.encode(
        JSON.stringify({ entityId: this.id, componentId: component.id }),
      ),
    );
    return sc.decode(reply.data) === "true";
  }

  async getComponent<T extends Component>(
    component: T,
  ): Promise<ComponentProxy<T> | null> {
    return null as ComponentProxy<T>;
  }
}
