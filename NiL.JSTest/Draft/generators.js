/////////////////////////////////////////////////////////////

function* g0()
{
    yield 1;
}

function g0_t()
{
    var $val = undefined;
    var $done = false;
    var $state = 0;
    function body(){
        switch ($state)
        {
            case 0:
                $val = 1;
                $state = 1;
                $done = true;
                break;
            case 1:
                $val = undefined;
        }
    };
    return {
        get value() { return $val },
        get done() { return $done },
        get next() {
            return body;
        }
    };
}