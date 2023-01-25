if (JSON.stringify({ a: null }) !== '{"a":null}')
    console.error("JSON.stringify ignores null");