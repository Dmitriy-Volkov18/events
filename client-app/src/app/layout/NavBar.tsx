import { observer } from 'mobx-react-lite';
import { Link, NavLink } from 'react-router-dom';
import { Button, Container, Menu, Image, Dropdown } from 'semantic-ui-react'
import { useStore } from '../stores/store';

const NavBar = () => {
    const {userStore: {user, logout}} = useStore();

    return (
        <Menu inverted fixed="top">
            <Container>
                <Menu.Item as={NavLink} to='/' header>
                    Home
                </Menu.Item>
                <Menu.Item as={NavLink} to="/activities" name="Events" />
                <Menu.Item>
                    <Button as={NavLink} to="/createActivity" positive content="Create Event"></Button>
                </Menu.Item>
                <Menu.Item position='right'>
                    <Image src={user?.image || '/assets/user.png'} avatar spaced='right' />
                    <Dropdown pointing='top left' text={user?.username}>
                        <Dropdown.Menu>
                            <Dropdown.Item as={Link} to={`/profiles/${user?.username}`} text='My Profile' icon='user'></Dropdown.Item>
                            <Dropdown.Item onClick={logout} text='Logout' icon='power'></Dropdown.Item>
                        </Dropdown.Menu>
                    </Dropdown>
                </Menu.Item>
            </Container>
        </Menu>
    )
}

export default observer(NavBar)
