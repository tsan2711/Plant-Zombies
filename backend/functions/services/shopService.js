import * as functions from "firebase-functions";
import admin from "firebase-admin";

const db = admin.firestore();
const shopRef = db.collection("shop");

export const addItem = functions.https.onCall(async (data) => {
  await shopRef.doc(data.itemId).set(data);
  return { success: true, item: data };
});

export const getShopItems = functions.https.onCall(async () => {
  const snapshot = await shopRef.get();
  return snapshot.docs.map(doc => doc.data());
});

export const updateItem = functions.https.onCall(async (data) => {
  await shopRef.doc(data.itemId).update(data.updates);
  return { success: true, message: "Item updated" };
});

export const deleteItem = functions.https.onCall(async (data) => {
  await shopRef.doc(data.itemId).delete();
  return { success: true, message: "Item deleted" };
});
