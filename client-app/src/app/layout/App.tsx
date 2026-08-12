import React, { useEffect } from 'react';
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard';
import { observer } from 'mobx-react-lite';
import HomePage from '../../features/home/HomePage';
import { Routes, Route, useNavigate } from 'react-router-dom';
import ActivityForm from '../../features/activities/form/ActivityForm';
import ActivityDetails from '../../features/activities/details/ActivityDetails';
import { ToastContainer } from 'react-toastify';
import NotFound from '../../features/errors/NotFound';
import ServerError from '../../features/errors/ServerError';
import { useStore } from '../stores/store';
import LoadingComponent from './LoadingComponent';
import ModalContainer from '../common/modals/ModalContainer';
import ProfilePage from '../../features/profiles/ProfilePage';
import PrivateRoute from './PrivateRoute';
import AppLayout from './AppLayout';
import { setNavigate } from '../router';

function App() {
  const {commonStore, userStore} = useStore();

  useEffect(() => {
    if(commonStore.token){
      userStore.getUser().finally(() => commonStore.setAppLoaded())
    }else{
      commonStore.setAppLoaded();
    }
  }, [commonStore, userStore]);

    const navigate = useNavigate();

  useEffect(() => {
    setNavigate(navigate);
  }, [navigate]);

  if(!commonStore.appLoaded) return <LoadingComponent content="Loading app"/>
  return (
    <>
      <ToastContainer position="bottom-right" hideProgressBar />
      <ModalContainer />
      <Routes>
        <Route path="/" element={ <HomePage /> } />
        <Route element={ <AppLayout/> }>
          <Route
            path="activities"
            element={<PrivateRoute><ActivityDashboard /></PrivateRoute>}
          />
          <Route
            path="activities/:id"
            element={<PrivateRoute><ActivityDetails /></PrivateRoute>}
          />
          <Route
            path="createActivity"
            element={<PrivateRoute><ActivityForm /></PrivateRoute>}
          />
          <Route
            path="manage/:id"
            element={<PrivateRoute><ActivityForm /></PrivateRoute>}
          />
          <Route
            path="profiles/:username"
            element={<PrivateRoute><ProfilePage /></PrivateRoute>}
          />
          
          <Route path="server-error" element={<ServerError />} />

          {/* 404 */}
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </>
  );
}

export default observer(App);
