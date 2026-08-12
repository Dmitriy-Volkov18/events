import './app/layout/styles.css';
import App from './app/layout/App';
//import reportWebVitals from './reportWebVitals';
import 'semantic-ui-css/semantic.min.css';
import { store, StoreContext } from './app/stores/store';
import { BrowserRouter } from 'react-router-dom';
import 'react-calendar/dist/Calendar.css'
import 'react-toastify/dist/ReactToastify.css';
import 'react-datepicker/dist/react-datepicker.css';
import ScrollToTop from './app/layout/ScrollToTop';
import { createRoot } from 'react-dom/client';

const container = document.getElementById('root');
if (container) {
  const root = createRoot(container); // создаём корневой контейнер
  root.render(
    <StoreContext.Provider value={store}>
      <BrowserRouter>
        <ScrollToTop />
        <App />
      </BrowserRouter>
    </StoreContext.Provider>
  );
}

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
//reportWebVitals();
