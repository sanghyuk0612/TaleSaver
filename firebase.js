// Firebase SDK 가져오기
import { initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";

// Firebase 설정 정보
const firebaseConfig = {
  apiKey: "AIzaSyCTEXMlsJSGF7lIdVt-Hn9BITyx_8Pyu20",
  authDomain: "tale-saver.firebaseapp.com",
  projectId: "tale-saver",  
  storageBucket: "tale-saver.firebasestorage.app",
  messagingSenderId: "870744593385",
  appId: "1:870744593385:web:3c65b1a42e1ec62fc27178",
  measurementId: "G-3JK22CK2TJ"
};
// Firebase 초기화
const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app);
export { app, analytics };
