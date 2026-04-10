import { connect } from "nats";
import { EntityHandler } from "./entity-handler.js";
export { EntityRepository } from "./entity-repository.js";

const nc = await connect({ servers: process.env.NATS_URL });
const handler = new EntityHandler(nc);

await handler.listen();

console.log("Backend listening on NATS");
