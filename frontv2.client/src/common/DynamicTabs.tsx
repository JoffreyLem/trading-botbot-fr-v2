import React, { ReactElement, useState } from "react";

interface TabPanelProps {
  title: string;
  children: React.ReactNode;
}

const TabPanel: React.FC<TabPanelProps> = ({ children }) => {
  return <div>{children}</div>;
};

interface DynamicTabsProps {
  children: ReactElement<TabPanelProps>[] | ReactElement<TabPanelProps>;
}

const DynamicTabs: React.FC<DynamicTabsProps> & {
  TabPanel: React.FC<TabPanelProps>;
} = ({ children }) => {
  const [activeTab, setActiveTab] = useState(0);
  const childArray = React.Children.toArray(
    children,
  ) as ReactElement<TabPanelProps>[];

  const renderActiveTabContent = () => {
    if (childArray[activeTab]) {
      return childArray[activeTab].props.children;
    }
    return null; // ou une certaine page par défaut
  };

  return (
    <div>
      <ul className="nav nav-tabs">
        {childArray.map((child, index) => (
          <li key={index} className="nav-item">
            <a
              className={`nav-link ${index === activeTab ? "active" : ""}`}
              onClick={() => setActiveTab(index)}
            >
              {child.props.title}
            </a>
          </li>
        ))}
      </ul>
      <div className="tab-content">{renderActiveTabContent()}</div>
    </div>
  );
};

DynamicTabs.TabPanel = TabPanel;

export default DynamicTabs;
