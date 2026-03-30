import { connect } from "nats";
import { EntityHandler } from "./entity-handler.js";
import {
  nameWorker,
  parentWorker,
  poseWorker,
  followPoseWorker,
} from "@ardo314/in-memory-workers";
export { EntityRepository } from "./entity-repository.js";

const nc = await connect();
const handler = new EntityHandler(nc);

handler.registerWorker(nameWorker);
handler.registerWorker(parentWorker);
handler.registerWorker(poseWorker);
handler.registerWorker(followPoseWorker);

await handler.listen();

console.log("Backend listening on NATS");
