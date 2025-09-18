import * as functions from "firebase-functions";
import admin from "firebase-admin";

const db = admin.firestore();
const zombiesRef = db.collection("zombies");

export const createZombie = functions.https.onCall(async (data) => {
  await zombiesRef.doc(data.zombieId).set(data);
  return { success: true, zombie: data };
});

export const getAllZombies = functions.https.onCall(async () => {
  const snapshot = await zombiesRef.get();
  return snapshot.docs.map(doc => doc.data());
});

export const getZombieById = functions.https.onCall(async (data) => {
  const doc = await zombiesRef.doc(data.zombieId).get();
  if (!doc.exists) throw new functions.https.HttpsError("not-found", "Zombie not found");
  return doc.data();
});

export const updateZombie = functions.https.onCall(async (data) => {
  await zombiesRef.doc(data.zombieId).update(data.updates);
  return { success: true, message: "Zombie updated" };
});

export const deleteZombie = functions.https.onCall(async (data) => {
  await zombiesRef.doc(data.zombieId).delete();
  return { success: true, message: "Zombie deleted" };
});
