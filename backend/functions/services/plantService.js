import * as functions from "firebase-functions";
import admin from "firebase-admin";

const db = admin.firestore();
const plantsRef = db.collection("plants");

// Tạo Plant
export const createPlant = functions.https.onCall(async (data) => {
  await plantsRef.doc(data.plantId).set(data);
  return { success: true, message: "Plant created", plant: data };
});

// Lấy tất cả Plants
export const getAllPlants = functions.https.onCall(async () => {
  const snapshot = await plantsRef.get();
  return snapshot.docs.map(doc => doc.data());
});

// Lấy 1 Plant
export const getPlantById = functions.https.onCall(async (data) => {
  const doc = await plantsRef.doc(data.plantId).get();
  if (!doc.exists) throw new functions.https.HttpsError("not-found", "Plant not found");
  return doc.data();
});

// Update Plant
export const updatePlant = functions.https.onCall(async (data) => {
  await plantsRef.doc(data.plantId).update(data.updates);
  return { success: true, message: "Plant updated" };
});

// Xóa Plant
export const deletePlant = functions.https.onCall(async (data) => {
  await plantsRef.doc(data.plantId).delete();
  return { success: true, message: "Plant deleted" };
});
