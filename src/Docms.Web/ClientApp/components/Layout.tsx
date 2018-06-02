import * as React from 'react';
import { NavMenu } from './NavMenu';
import { SearchBar } from './SearchBar';

export interface LayoutProps {
    children?: React.ReactNode;
}

export class Layout extends React.Component<LayoutProps, {}> {
    public render() {
        return <div className='container-fluid'>
            <NavMenu />
            <div className='row'>
                <div className='col-sm-9'>
                    {this.props.children}
                </div>
                <div className='col-sm-3'>
                    <SearchBar />
                </div>
            </div>
        </div>;
    }
}
