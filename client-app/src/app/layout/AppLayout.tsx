import { Outlet } from 'react-router-dom';
import NavBar from './NavBar';
import { Container } from 'semantic-ui-react';

export default function AppLayout() {
  return (
    <>
      <NavBar />
      <Container style={{ marginTop: '7em' }}>
        <Outlet />
      </Container>
    </>
  );
}
