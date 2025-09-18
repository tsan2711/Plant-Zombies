import * as functions from "firebase-functions";
import admin from "firebase-admin";

const db = admin.firestore();
const sessionsRef = db.collection("gamesessions");

export const createSession = functions.https.onCall(async (data) => {
  const sessionId = data.sessionId || db.collection("gamesessions").doc().id;
  data.sessionId = sessionId;
  await sessionsRef.doc(sessionId).set(data);
  return { success: true, session: data };
});

export const getSession = functions.https.onCall(async (data) => {
  const doc = await sessionsRef.doc(data.sessionId).get();
  if (!doc.exists) throw new functions.https.HttpsError("not-found", "Session not found");
  return doc.data();
});

export const updateSession = functions.https.onCall(async (data) => {
  await sessionsRef.doc(data.sessionId).update(data.updates);
  return { success: true, message: "Session updated" };
});

export const deleteSession = functions.https.onCall(async (data) => {
  await sessionsRef.doc(data.sessionId).delete();
  return { success: true, message: "Session deleted" };
});
