import * as React from 'react';
import { Link, NavLink } from 'react-router-dom';

export class NavMenu extends React.Component<{}, {}> {
    public render() {
        return <div className='main-nav'>
            <div className='navbar navbar-inverse navbar-fixed-top'>
                <div className='navbar-header'>
                    <button type='button' className='navbar-toggle' data-toggle='collapse' data-target='.navbar-collapse'>
                        <span className='sr-only'>Toggle navigation</span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                    </button>
                    <Link className='navbar-brand' to={'/'}>文書管理システム</Link>
                </div>
                <div className='navbar-collapse collapse'>
                    <ul className='nav navbar-nav navbar-right'>
                        <li className='dropdown'>
                            <a href='#' className='dropdown-toggle' data-toggle='dropdown'>@name <b className='caret'></b></a>
                            <ul className='dropdown-menu'>
                                <li><a href='/account/logout'>ログアウト</a></li>
                            </ul>
                        </li>
                    </ul>
                </div>
            </div>
        </div>;
    }
}
