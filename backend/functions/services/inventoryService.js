import * as functions from "firebase-functions";
import admin from "firebase-admin";

const db = admin.firestore();
const inventoriesRef = db.collection("inventories");

export const createInventory = functions.https.onCall(async (data) => {
  await inventoriesRef.doc(data.uid).set(data);
  return { success: true, inventory: data };
});

export const getInventory = functions.https.onCall(async (data) => {
  const doc = await inventoriesRef.doc(data.uid).get();
  if (!doc.exists) throw new functions.https.HttpsError("not-found", "Inventory not found");
  return doc.data();
});

export const updateInventory = functions.https.onCall(async (data) => {
  await inventoriesRef.doc(data.uid).update(data.updates);
  return { success: true, message: "Inventory updated" };
});
