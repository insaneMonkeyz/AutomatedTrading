## ������� ������������ ��� ������������ � ���� ������� � �����

������ ������������� ������ � ���������� ����� ����� �������� Quik ��������� ��� Lua C Api ���������

����� ���������������� ������� ����������� ����������� [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs)

��������� ��� ������ � ������ 
[Quik.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/Quik.cs) � 
[IQuikImplementation.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuikImplementation.cs)

����� ������� ������� ����� ������ ������� ��������, �������������� ������ c ��������������� ������������� Lua:

1) ����� ������� ������� ���������� - �������������� ��������� [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs) ����������� ����������� � �������� ������ ���������� - ����������� ������, ��������� ���������� �� ������������, ����� ���

2) ��������� ���� - [���������� ������](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders). ����� ������� ������ ��������� ���������� ([������, ������, �������� ������������, ��������� ���](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Entities)) 
���������� � ����������� ��� �������� � ��������. 

3) ��� ����� �������� ���� ���������� - [������� ��� ������ API ���������](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders/QuikApiWrappers). ����� ������� ������ ���������� ����������� � DTO �������� ���������.

4) ��������� ���� - [������ ��� Lua C API](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Lua). ������������ ����� ������ ����������� ������ Lua � ������ ���������. ����� ���� �������������� ����� ������� � ����������.