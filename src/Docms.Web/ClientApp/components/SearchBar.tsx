import * as React from 'react';
import { Link } from 'react-router-dom';

export class SearchBar extends React.Component<{}, {}> {
    public render() {
        return <div className='search-bar'>
            検索ボックス
            <Link to={'/documents/1'}>Test</Link>
        </div>;
    }
}
