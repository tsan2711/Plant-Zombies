import * as functions from "firebase-functions";
import admin from "firebase-admin";

const db = admin.firestore();
const playersRef = db.collection("players");

export const createPlayer = functions.https.onCall(async (data) => {
  await playersRef.doc(data.uid).set(data);
  return { success: true, player: data };
});

export const getPlayer = functions.https.onCall(async (data) => {
  const doc = await playersRef.doc(data.uid).get();
  if (!doc.exists) throw new functions.https.HttpsError("not-found", "Player not found");
  return doc.data();
});

export const updatePlayer = functions.https.onCall(async (data) => {
  await playersRef.doc(data.uid).update(data.updates);
  return { success: true, message: "Player updated" };
});
