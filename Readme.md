## A quick guide to my work with the code

The project provides access to the Moscow Stock Exchange via the Quik Terminal using its Lua C API.

Its functionality is represented by the [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs)
 interface, which is implemented by the classes
[Quik.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/Quik.cs) & 
[IQuikImplementation.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuikImplementation.cs)

Data is exchanged through a series of layer classes that abstract away the work of low-level 
LUA operations:

1) The highest level of abstraction is the [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs)
 interface mentioned above. 
 It acts as a conductor to the main logic of the application - placing new orders, 
 requesting information about securities, getting quotes, etc.


2) The next layer is [Data Providers](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders).
This is where the business entities of the application, such as [trades, orders, quotes, securities](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Entities)
are assembled and disassembled before being sent to the terminal.

3) An even lower abstraction layer - [a wrapper for the terminal API](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders/QuikApiWrappers).
Here objects are translated to DTO objects of the terminal.

4) The last abstraction level - [a wrapper for the Lua C API](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Lua).
Represents the logic operating on the Lua stack in the terminal's memory.
This is where data exchange occurs.

## Краткий путеводитель для ознакомления с моей работой с кодом

Проект предоставляет доступ к московской бирже через терминал Quik используя его Lua C Api интерфейс

Фасад функциональности проекта представлен интерфейсом [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs)

Реализуют его классы в файлах 
[Quik.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/Quik.cs) и 
[IQuikImplementation.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuikImplementation.cs)

Обмен данными ведется через череду классов прослоек, абстрагирующих работу c низкоуровневыми конструкциями Lua:

1) Самый высокий уровень абстракции - вышеупомянутый интерфейс [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs) выступающий проводником к основной логике приложения - выставление заявок, получение информации по инструментам, ценам итд

2) Следующий слой - [провайдеры данных](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders). Здесь объекты бизнес сущностей приложения ([сделок, заявок, торговых инструментов, котировок итд](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Entities)) 
собираются и разбираются для загрузки в терминал. 

3) Еще более глубокий слой абстракции - [Обертка для бизнес API терминала](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders/QuikApiWrappers). Здесь объекты нашего приложения переводятся в DTO объектов терминала.

4) Последний слой - [Обёртка для Lua C API](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Lua/LuaWrap.cs). Представляет собой логику оперирующую стеком Lua в памяти терминала. Через него осуществляется обмен данными с терминалом.
