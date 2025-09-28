// firebase.js
import { initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";

const firebaseConfig = {
    apiKey: "AIzaSyDFYN_fQoTsI7fx4wn4_SMRClut50s7klQ",
    authDomain: "lungisa-e03bd.firebaseapp.com",
    databaseURL: "https://lungisa-e03bd-default-rtdb.firebaseio.com",
    projectId: "lungisa-e03bd",
    storageBucket: "lungisa-e03bd.firebasestorage.app",
    messagingSenderId: "781545590155",
    appId: "1:781545590155:web:265a64bb2917efc7da338e",
    measurementId: "G-M5BN5X67S9"
};

const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app);

export default app;
